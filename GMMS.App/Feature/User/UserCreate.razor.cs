using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.User.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.User
{
    public partial class UserCreate : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private CreateUserRequestModel request = new()
        {
            Role = "Admin",
            IsActive = true
        };

        private string password = string.Empty;

        private bool isLoading;
        private string? errorMessage;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                errorMessage = "Username is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Password is required.";
                return;
            }

            isLoading = true;
            errorMessage = null;

            try
            {
                request.Password = password;
                var result = await ApiService.CreateUserAsync<CreateUserRequestModel, Result<UserModel>>(request);

                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("User created successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to create user.";
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
