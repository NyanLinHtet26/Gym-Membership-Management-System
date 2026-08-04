using GMMS.App.Models;
using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Auth.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GMMS.App.Feature.Auth
{
    public partial class Login : ComponentBase
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private string userName = string.Empty;
        private string password = string.Empty;
        private bool showPassword;
        private bool rememberMe;
        private bool isLoading;
        private string? errorMessage;

        private void TogglePassword()
        {
            showPassword = !showPassword;
        }

        private void HandleForgotPassword()
        {
            Snackbar.Add("Please contact the gym owner to reset your password.", Severity.Info);
        }

        private async Task HandleEnterKey(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await HandleLogin();
            }
        }

        private async Task HandleLogin()
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Username and password are required.";
                return;
            }

            isLoading = true;

            try
            {
                var request = new LoginRequestModel
                {
                    UserName = userName,
                    Password = password
                };

                var result = await ApiService.LoginAsync<LoginRequestModel, Result<LoginDataModel>>(request);

                if (result?.IsSuccess == true && result.Data is not null)
                {
                    var userModel = new LoginResponseModel
                    {
                        UserId = result.Data.User.UserId,
                        UserName = result.Data.User.UserName,
                        Role = result.Data.User.Role,
                        MustChangePassword = result.Data.User.MustChangePassword
                    };

                    AuthTokenStore.SetAuth(result.Data.AccessToken, userModel);

                    if (result.Data.User.MustChangePassword)
                    {
                        Navigation.NavigateTo("/change-password");
                    }
                    else
                    {
                        Navigation.NavigateTo("/member-list");
                    }
                }
                else
                {
                    errorMessage = result?.Message ?? "Login failed.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
