using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.User.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.User
{
    public partial class UserEdit : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int UserId { get; set; }

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private UpdateUserRequestModel request = new();
        private bool isLoading = true;
        private bool isSaving;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var result = await ApiService.GetUserDetailsAsync<Result<UserModel>>(UserId);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    request.UserId = result.Data.UserId;
                    request.UserName = result.Data.UserName;
                    request.Role = result.Data.Role;
                    request.IsActive = result.Data.IsActive;
                }
                else
                {
                    errorMessage = result?.Message ?? "User not found.";
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
            MudDialog.Cancel();
        }

        private async Task Update()
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                errorMessage = "Username is required.";
                return;
            }

            isSaving = true;
            errorMessage = null;

            try
            {
                var result = await ApiService.UpdateUserAsync<UpdateUserRequestModel, Result<UserModel>>(UserId, request);
                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("User updated successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to update user.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isSaving = false;
            }
        }
    }
}
