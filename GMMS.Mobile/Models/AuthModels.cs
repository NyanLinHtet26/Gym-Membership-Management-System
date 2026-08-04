namespace GMMS.Mobile.Models;

public sealed class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginUserData
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
}

public sealed class TokenData
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public sealed class LoginResponseData
{
    public LoginUserData User { get; set; } = new();
    public TokenData AccessToken { get; set; } = new();
    public TokenData RefreshToken { get; set; } = new();
}

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
