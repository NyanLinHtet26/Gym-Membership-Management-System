using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Member.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Member
{
    public partial class MemberDelete : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int MemberId { get; set; }

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private MemberModel? member;
        private bool isLoading = true;
        private bool isDeleting;
        private bool loadFailed;
        private bool confirmed;
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
                var result = await ApiService.GetMemberDetailsAsync<Result<MemberModel>>(MemberId);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    member = result.Data;
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to load member.";
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

        private async Task ConfirmDelete()
        {
            if (!confirmed)
            {
                return;
            }

            isDeleting = true;
            errorMessage = null;
            StateHasChanged();

            try
            {
                var result = await ApiService.DeleteMemberAsync<Result<bool>>(MemberId);
                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("Member deleted successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to delete member.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isDeleting = false;
            }
        }
    }
}
