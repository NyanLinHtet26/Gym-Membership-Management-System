using GMMS.Domain.Features.Auth.Models;

namespace GMMS.App.Services
{
    public class AuthTokenStore
    {
        public string? AccessToken { get; private set; }
        public LoginResponseModel? CurrentUser { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);
        public bool MustChangePassword => CurrentUser?.MustChangePassword ?? false;

        public void SetAuth(string accessToken, LoginResponseModel user)
        {
            AccessToken = accessToken;
            CurrentUser = user;
        }

        public void Clear()
        {
            AccessToken = null;
            CurrentUser = null;
        }
    }
}
