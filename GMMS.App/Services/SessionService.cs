using Microsoft.AspNetCore.Components;

namespace GMMS.App.Services
{
    public class SessionService
    {
        private readonly ApiService _api;
        private readonly AuthTokenStore _auth;
        private readonly NavigationManager _nav;

        public SessionService(ApiService api, AuthTokenStore auth, NavigationManager nav)
        {
            _api = api;
            _auth = auth;
            _nav = nav;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _api.LogoutAsync<object>();
            }
            catch
            {
            }
            _auth.Clear();
            _nav.NavigateTo("/login", forceLoad: true);
        }

        public static string GetPageTitle(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) return "Home";
            var path = new Uri(uri).AbsolutePath.TrimEnd('/');

            switch (path)
            {
                case "":
                case "/":
                    return "Dashboard";
                case "/member-list":
                    return "Members";
                case "/membership-list":
                case "/membership-list-all":
                    return "Memberships";
                case "/payment-list":
                    return "Payments";
                case "/user-list":
                    return "Users";
                case "/settings":
                    return "Settings";
                case "/change-password":
                    return "Change Password";
                case "/login":
                    return "Sign In";
                default:
                    return "Home";
            }
        }
    }
}
