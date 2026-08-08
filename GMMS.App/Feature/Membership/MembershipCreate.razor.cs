using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Member.Models;
using GMMS.Domain.Features.MemberShip.Models;
using GMMS.Domain.Features.MemberShipPlan.Models;
using GMMS.Domain.Features.PaymentMethod.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Membership
{
    public partial class MembershipCreate : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int MemberId { get; set; }

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private CreateMemberShipRequestModel request = new();
        private List<MemberModel> members = new();
        private List<MemberShipPlanModel> plans = new();
        private List<PaymentMethodModel> paymentMethods = new();
        private bool isLoadingData = true;
        private bool isSaving;
        private string? errorMessage;

        private string? selectedMemberName;

        private string _memberStr
        {
            get => request.MemberId > 0 ? request.MemberId.ToString() : "";
            set => request.MemberId = int.TryParse(value, out var id) ? id : 0;
        }

        private MemberShipPlanModel? GetSelectedPlan()
            => plans.FirstOrDefault(p => p.MemberShipPlanId == request.MembershipPlanId);

        private string _planStr
        {
            get => request.MembershipPlanId > 0 ? request.MembershipPlanId.ToString() : "";
            set
            {
                request.MembershipPlanId = int.TryParse(value, out var id) ? id : 0;
                request.Amount = GetSelectedPlan()?.Price ?? 0;
            }
        }

        private string _paymentMethodStr
        {
            get => request.PaymentMethodId > 0 ? request.PaymentMethodId.ToString() : "";
            set => request.PaymentMethodId = int.TryParse(value, out var id) ? id : 0;
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var memberResult = await ApiService.GetMemberListAsync<Result<MemberListResponseModel>>(1, 100);
                if (memberResult?.IsSuccess == true && memberResult.Data is not null)
                    members = memberResult.Data.Members ?? new();
                else
                    errorMessage = memberResult?.Message ?? "Failed to load members.";

                var planResult = await ApiService.GetMembershipPlanListAsync<Result<MemberShipPlanListResponseModel>>(1, 100);
                if (planResult?.IsSuccess == true && planResult.Data is not null)
                    plans = (planResult.Data.MemberShipPlans ?? new()).Where(p => p.IsActive).ToList();
                else if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = planResult?.Message ?? "Failed to load membership plans.";

                var methodResult = await ApiService.GetPaymentMethodListAsync<Result<PaymentMethodListResponseModel>>(1, 100);
                if (methodResult?.IsSuccess == true && methodResult.Data is not null)
                    paymentMethods = (methodResult.Data.PaymentMethods ?? new()).Where(p => p.IsActive).ToList();
                else if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = methodResult?.Message ?? "Failed to load payment methods.";

                if (MemberId > 0)
                {
                    request.MemberId = MemberId;
                    var member = members.FirstOrDefault(m => m.MemberId == MemberId);
                    selectedMemberName = member is not null
                        ? $"{member.Name} ({member.MemberCode})"
                        : $"Member #{MemberId}";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isLoadingData = false;
                StateHasChanged();
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Save()
        {
            if (request.MemberId <= 0)
            {
                errorMessage = "Please select a member.";
                return;
            }
            if (request.MembershipPlanId <= 0)
            {
                errorMessage = "Please select a membership plan.";
                return;
            }
            if (request.PaymentMethodId <= 0)
            {
                errorMessage = "Please select a payment method.";
                return;
            }

            isSaving = true;
            errorMessage = null;

            try
            {
                var result = await ApiService.CreateMembershipAsync<CreateMemberShipRequestModel, Result<MembershipDetailModel>>(request);

                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("Membership created successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to create membership.";
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