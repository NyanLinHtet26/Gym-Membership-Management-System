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

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        private UserModel? user;
        private UpdateUserRequestModel request = new();
        private bool isLoading;
        private bool loadFailed;
        private bool isSaving;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            isLoading = true;
            loadFailed = false;
            errorMessage = null;

            try
            {
                var result = await ApiService.GetUserDetailsAsync<Result<UserModel>>(UserId);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    user = result.Data;
                    request.UserId = user.UserId;
                    request.UserName = user.UserName;
                    request.Role = user.Role;
                    request.IsActive = user.IsActive;
                }
                else
                {
                    errorMessage = result?.Message ?? "User not found.";
                    loadFailed = true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                loadFailed = true;
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task RetryAsync() => await LoadAsync();

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
