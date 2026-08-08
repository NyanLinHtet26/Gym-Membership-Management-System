using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.MemberShip.Models;
using GMMS.Domain.Features.MemberShipPlan.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Membership
{
    public partial class MembershipEdit : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int MembershipId { get; set; }

        [Parameter]
        public int MemberId { get; set; }

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private UpdateMembershipRequestModel request = new();
        private MembershipDetailModel? detail;
        private List<MemberShipPlanModel> plans = new();
        private bool isLoadingData = true;
        private bool isSaving;
        private bool loadFailed;
        private string? errorMessage;

        private DateOnly? newEndDate;

        private MemberShipPlanModel? SelectedPlan =>
            plans.FirstOrDefault(p => p.MemberShipPlanId == request.MembershipPlanId);

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            isLoadingData = true;
            loadFailed = false;
            errorMessage = null;

            try
            {
                var result = await ApiService.GetMembershipDetailsAsync<Result<MembershipDetailModel>>(MembershipId);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    detail = result.Data;
                    request.MembershipId = result.Data.MembershipId;
                    request.MembershipPlanId = result.Data.MembershipPlanId;
                    if (MemberId <= 0)
                        MemberId = result.Data.MemberId;
                }
                else
                {
                    errorMessage = result?.Message ?? "Membership not found.";
                    loadFailed = true;
                }

                var planResult = await ApiService.GetMembershipPlanListAsync<Result<MemberShipPlanListResponseModel>>(1, 100);
                if (planResult?.IsSuccess == true && planResult.Data is not null)
                    plans = planResult.Data.MemberShipPlans ?? new();
                else if (!loadFailed)
                    errorMessage = planResult?.Message ?? "Failed to load membership plans.";

                if (detail is not null && plans is not null)
                    RecalcEndDate();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                loadFailed = true;
            }
            finally
            {
                isLoadingData = false;
                StateHasChanged();
            }
        }

        private async Task RetryAsync() => await LoadAsync();

        private void OnPlanValueChanged(int membershipPlanId)
        {
            request.MembershipPlanId = membershipPlanId;
            RecalcEndDate();
        }

        private void RecalcEndDate()
        {
            if (detail is null || plans is null || request.MembershipPlanId <= 0)
            {
                newEndDate = null;
                return;
            }
            var plan = plans.FirstOrDefault(p => p.MemberShipPlanId == request.MembershipPlanId);
            newEndDate = plan is not null
                ? detail.StartDate.AddDays(plan.DurationDays)
                : null;
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Update()
        {
            if (request.MembershipPlanId <= 0)
            {
                errorMessage = "Please select a membership plan.";
                return;
            }

            isSaving = true;
            errorMessage = null;

            try
            {
                var result = await ApiService.UpdateMembershipAsync<UpdateMembershipRequestModel, Result<MembershipDetailModel>>(MembershipId, request);
                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("Membership updated successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to update membership.";
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

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "Active" => Color.Success,
                "Pending" => Color.Warning,
                "Expired" => Color.Error,
                _ => Color.Default
            };
        }
    }
}
