using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Member.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Member
{
    public partial class MemberEdit : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int MemberId { get; set; }

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private UpdateMemberRequestModel request = new();
        private bool isLoading = true;
        private bool isSaving;
        private bool loadFailed;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var result = await ApiService.GetMemberDetailsAsync<Result<MemberModel>>(MemberId);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    request.MemberId = result.Data.MemberId;
                    request.MemberCode = result.Data.MemberCode;
                    request.Name = result.Data.Name;
                }
                else
                {
                    errorMessage = result?.Message ?? "Member not found.";
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

        private void ConvertMemberCodeUpper()
        {
            request.MemberCode = request.MemberCode?.ToUpperInvariant() ?? "";
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Update()
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

            isSaving = true;
            errorMessage = null;

            try
            {
                var result = await ApiService.UpdateMemberAsync<UpdateMemberRequestModel, Result<MemberModel>>(MemberId, request);
                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("Member updated successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to update member.";
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