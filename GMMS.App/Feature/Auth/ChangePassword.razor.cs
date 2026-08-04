using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Auth.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GMMS.App.Feature.Auth
{
    public partial class ChangePassword : ComponentBase
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

        private string currentPassword = string.Empty;
        private string newPassword = string.Empty;
        private string confirmNewPassword = string.Empty;
        private bool showCurrent;
        private bool showNew;
        private bool showConfirm;
        private bool isLoading;
        private string? errorMessage;
        private string? successMessage;

        private void ToggleCurrent()
        {
            showCurrent = !showCurrent;
        }

        private void ToggleNew()
        {
            showNew = !showNew;
        }

        private void ToggleConfirm()
        {
            showConfirm = !showConfirm;
        }

        private async Task HandleEnterKey(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await HandleChangePassword();
            }
        }

        private async Task HandleChangePassword()
        {
            errorMessage = null;
            successMessage = null;

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                errorMessage = "All fields are required.";
                return;
            }

            if (newPassword != confirmNewPassword)
            {
                errorMessage = "New password and confirmation do not match.";
                return;
            }

            if (newPassword.Length < 6)
            {
                errorMessage = "New password must be at least 6 characters.";
                return;
            }

            isLoading = true;

            try
            {
                var request = new ChangePasswordRequestModel
                {
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmNewPassword = confirmNewPassword
                };

                var result = await ApiService.ChangePasswordAsync<ChangePasswordRequestModel, Result<object>>(request);

                if (result?.IsSuccess == true)
                {
                    AuthTokenStore.Clear();
                    successMessage = "Password changed successfully. Redirecting to login...";
                    await Task.Delay(2000);
                    Navigation.NavigateTo("/login", forceLoad: true);
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to change password.";
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

        private void Cancel()
        {
            Navigation.NavigateTo("/login");
        }
    }
}
