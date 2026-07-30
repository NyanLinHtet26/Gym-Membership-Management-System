namespace GMMS.App.Models;

public class LoginDataModel
{
    public LoginUserModel User { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
}

public class LoginUserModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool MustChangePassword { get; set; }
}
