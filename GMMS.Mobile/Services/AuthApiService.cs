using GMMS.Mobile.Models;

namespace GMMS.Mobile.Services;

public sealed class AuthApiService : ApiServiceBase
{
    private const string LoginEndpoint = "/api/Auth/login";
    private const string RefreshEndpoint = "/api/Auth/refresh";
    private const string LogoutEndpoint = "/api/Auth/logout";

    public AuthApiService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<LoginResponseData?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => PostAsync<LoginRequest, LoginResponseData>(LoginEndpoint, request, cancellationToken);

    public Task<LoginResponseData?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        => PostAsync<RefreshTokenRequest, LoginResponseData>(
            RefreshEndpoint,
            new RefreshTokenRequest { RefreshToken = refreshToken },
            cancellationToken);

    public Task<object?> LogoutAsync(CancellationToken cancellationToken = default)
        => PostAsync<object, object>(LogoutEndpoint, new object(), cancellationToken);
}
