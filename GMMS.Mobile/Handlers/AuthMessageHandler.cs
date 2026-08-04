using System.Net;
using System.Net.Http.Headers;
using GMMS.Mobile.Services;
using GMMS.Mobile.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace GMMS.Mobile.Handlers;

/// <summary>
/// Attaches the Bearer token to every request and transparently refreshes it
/// once when a request fails with 401.
/// </summary>
public sealed class AuthMessageHandler : DelegatingHandler
{
    private readonly TokenStorage _tokenStorage;
    private readonly IServiceProvider _services;
    private Task<bool>? _refreshTask;

    public AuthMessageHandler(TokenStorage tokenStorage, IServiceProvider services)
    {
        _tokenStorage = tokenStorage;
        _services = services;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || IsAuthEndpoint(request.RequestUri))
        {
            return response;
        }

        var refreshed = await TryRefreshAsync(cancellationToken);
        if (!refreshed)
        {
            return response;
        }

        var newToken = await _tokenStorage.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(newToken))
        {
            return response;
        }

        var retry = await CloneRequestAsync(request);
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
        response.Dispose();

        return await base.SendAsync(retry, cancellationToken);
    }

    private static bool IsAuthEndpoint(Uri? uri)
        => uri is not null && uri.AbsolutePath.Contains("/auth/", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Single-flight refresh: concurrent 401s share one refresh request.
    /// </summary>
    private Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        var current = _refreshTask;
        if (current is not null)
        {
            return current;
        }

        return _refreshTask = RefreshCoreAsync(cancellationToken);
    }

    private async Task<bool> RefreshCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            var authApi = _services.GetRequiredService<AuthApiService>();
            var result = await authApi.RefreshAsync(refreshToken, cancellationToken);
            if (result is null)
            {
                return false;
            }

            await _tokenStorage.UpdateTokensAsync(result.AccessToken.Token, result.RefreshToken.Token);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            _refreshTask = null;
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(body);
            if (request.Content.Headers.ContentType is not null)
            {
                clone.Content.Headers.ContentType = request.Content.Headers.ContentType;
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
