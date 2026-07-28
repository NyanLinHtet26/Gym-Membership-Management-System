namespace GMMS.Database.AppDbContextModels;

public partial class TblUserSession
{
    public int UserSessionId { get; set; }

    public Guid SessionId { get; set; }

    public int UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTime LoginTime { get; set; }

    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsExpired { get; set; }

    public virtual TblUser User { get; set; } = null!;
}