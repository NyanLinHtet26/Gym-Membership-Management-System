using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.MemberShip.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Membership
{
    public partial class MembershipDetail : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int MembershipId { get; set; }

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        private MembershipDetailModel? detail;
        private bool isLoading = true;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var result = await ApiService.GetMembershipDetailsAsync<Result<MembershipDetailModel>>(MembershipId);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    detail = result.Data;
                }
                else
                {
                    errorMessage = result?.Message ?? "Membership not found.";
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
    }
}
