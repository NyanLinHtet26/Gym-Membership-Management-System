using System.Net.Http.Json;
using System.Text.Json;
using GMMS.Mobile.Models;

namespace GMMS.Mobile.Services;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = 0)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Base HTTP layer: deserializes the API envelope ({ isSuccess, message, data })
/// and surfaces the server message as an <see cref="ApiException"/>.
/// </summary>
public abstract class ApiServiceBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    protected ApiServiceBase(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected async Task<TData?> GetAsync<TData>(string uri, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(uri, cancellationToken);
        return await DeserializeAsync<TData>(response, cancellationToken);
    }

    protected async Task<TData?> PostAsync<TRequest, TData>(string uri, TRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(uri, request, JsonOptions, cancellationToken);
        return await DeserializeAsync<TData>(response, cancellationToken);
    }

    private static async Task<TData?> DeserializeAsync<TData>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<TData>>(JsonOptions, cancellationToken);
            if (envelope?.IsSuccess == true)
            {
                return envelope.Data;
            }

            throw new ApiException(
                envelope?.Message ?? "The server rejected the request.",
                (int)response.StatusCode);
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new ApiException(
            ExtractMessage(raw) ?? $"Request failed with status {(int)response.StatusCode}.",
            (int)response.StatusCode);
    }

    private static string? ExtractMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}
