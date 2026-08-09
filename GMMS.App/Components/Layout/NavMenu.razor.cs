using GMMS.App.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Components.Layout
{
    public partial class NavMenu : ComponentBase
    {
        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [Inject]
        private SessionService Session { get; set; } = null!;

        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private bool _accountMenuOpen;

        private string Initial => AuthTokenStore.CurrentUser?.UserName is { Length: > 0 } name
            ? name[..1].ToUpperInvariant()
            : "?";

        private string RoleLabel => AuthTokenStore.CurrentUser?.Role switch
        {
            "Owner" => "Owner",
            "Admin" => "Admin",
            null => "",
            _ => AuthTokenStore.CurrentUser!.Role
        };

        private void GoToChangePassword()
        {
            Navigation.NavigateTo("/change-password");
        }

        private void GoToSettings()
        {
            Navigation.NavigateTo("/settings");
        }

        private async Task HandleLogout()
        {
            var dialog = await DialogService.ShowAsync<LogoutConfirm>("Log Out");
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await Session.LogoutAsync();
            }
        }
    }
}
