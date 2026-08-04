using System.Text.Json;
using GMMS.Mobile.Models;

namespace GMMS.Mobile.Storage;

/// <summary>
/// Persists auth tokens in the platform secure store (Windows DPAPI / Android EncryptedSharedPreferences).
/// </summary>
public sealed class TokenStorage
{
    private const string AccessTokenKey = "gmm_access_token";
    private const string RefreshTokenKey = "gmm_refresh_token";
    private const string UserKey = "gmm_user";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SaveAuthAsync(string accessToken, string refreshToken, LoginUserData user)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        await SaveUserAsync(user);
    }

    public async Task UpdateTokensAsync(string accessToken, string refreshToken)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
        await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
    }

    public Task<string?> GetAccessTokenAsync()
        => SecureStorage.Default.GetAsync(AccessTokenKey);

    public Task<string?> GetRefreshTokenAsync()
        => SecureStorage.Default.GetAsync(RefreshTokenKey);

    public async Task SaveUserAsync(LoginUserData user)
        => await SecureStorage.Default.SetAsync(UserKey, JsonSerializer.Serialize(user, JsonOptions));

    public async Task<LoginUserData?> GetUserAsync()
    {
        var json = await SecureStorage.Default.GetAsync(UserKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LoginUserData>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task ClearAsync()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(UserKey);
        await Task.CompletedTask;
    }

    public async Task<bool> HasSessionAsync()
        => !string.IsNullOrWhiteSpace(await GetRefreshTokenAsync());
}
