using GMMS.App.Services;
using Microsoft.AspNetCore.Components;

namespace GMMS.App.Feature.Setting
{
    public partial class Settings : ComponentBase
    {
        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

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
    }
}
