using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Member.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Member
{
    public partial class MemberCreate : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private CreateMemberRequestModel request = new();
        private bool isLoading;
        private string? errorMessage;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(request.MemberCode))
            {
                errorMessage = "Member Code is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                errorMessage = "Name is required.";
                return;
            }

            isLoading = true;
            errorMessage = null;

            try
            {
                var result = await ApiService.CreateMemberAsync<CreateMemberRequestModel, Result<MemberModel>>(request);

                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("Member created successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to create member.";
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