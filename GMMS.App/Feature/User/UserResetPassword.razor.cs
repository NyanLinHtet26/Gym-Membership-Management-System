using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.User.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.User
{
    public partial class UserResetPassword : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int UserId { get; set; }

        [Parameter]
        public string UserName { get; set; } = null!;

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private string newPassword = string.Empty;
        private bool isLoading;
        private string? errorMessage;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task ConfirmReset()
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                errorMessage = "New password is required.";
                return;
            }

            isLoading = true;
            errorMessage = null;

            try
            {
                var request = new ResetPasswordRequestModel
                {
                    UserId = UserId,
                    NewPassword = newPassword
                };

                var result = await ApiService.ResetUserPasswordAsync<ResetPasswordRequestModel, Result<bool>>(request);
                if (result?.IsSuccess == true)
                {
                    Snackbar.Add($"Password reset for {UserName}. User must change on next login.", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to reset password.";
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
