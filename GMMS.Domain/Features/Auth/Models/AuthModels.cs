namespace GMMS.Domain.Features.Auth.Models;

public class LoginRequestModel
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginResponseModel
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool MustChangePassword { get; set; }
}

public class LoginResultModel
{
    public LoginResponseModel User { get; set; } = new();

    public TokenResultModel Tokens { get; set; } = new();
}


public class ChangePasswordRequestModel
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class UserSessionModel
{
    public int UserSessionId { get; set; }

    public Guid SessionId { get; set; }

    public int UserId { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsActive { get; set; }
}
public class AccessTokenModel
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}


public class RefreshTokenModel
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}

public class TokenResultModel
{
    public AccessTokenModel AccessToken { get; set; } = new();

    public RefreshTokenModel RefreshToken { get; set; } = new();
}

public class RefreshTokenRequestModel
{
    public string RefreshToken { get; set; } = null!;
}