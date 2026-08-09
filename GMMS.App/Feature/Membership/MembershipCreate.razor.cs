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
        private bool loadFailed;
        private string? errorMessage;

        private string? selectedMemberName;
        private MemberShipModel? existingMembership;

        private DateOnly? CalculatedEndDate
            => GetSelectedPlan() is { } plan
                ? MyanmarDateTimeFormatter.TodayMyanmarDateOnly.AddDays(plan.DurationDays)
                : null;

        private MemberShipPlanModel? GetSelectedPlan()
            => plans.FirstOrDefault(p => p.MemberShipPlanId == request.MembershipPlanId);

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

                if (!string.IsNullOrEmpty(errorMessage))
                    loadFailed = true;

                if (MemberId > 0)
                {
                    request.MemberId = MemberId;
                    var member = members.FirstOrDefault(m => m.MemberId == MemberId);
                    selectedMemberName = member is not null
                        ? $"{member.Name} ({member.MemberCode})"
                        : $"Member #{MemberId}";
                    await LoadExistingMembershipAsync(MemberId);
                }
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

        private async Task LoadExistingMembershipAsync(int memberId)
        {
            existingMembership = null;
            if (memberId <= 0) return;

            try
            {
                var result = await ApiService.GetMembershipListAsync<Result<MemberShipListResponseModel>>(memberId, 1, 10);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    existingMembership = result.Data.MemberShips
                        .OrderBy(m => m.Status == "Expired" ? 1 : 0)
                        .ThenByDescending(m => m.EndDate)
                        .FirstOrDefault();
                }
            }
            catch
            {
                // Existing membership display is informational; ignore load failures.
            }
        }

        private async Task RetryAsync() => await LoadAsync();

        private void OnMemberValueChanged(int memberId)
        {
            request.MemberId = memberId;
            _ = LoadExistingMembershipAsync(memberId);
        }

        private void OnPlanValueChanged(int membershipPlanId)
        {
            request.MembershipPlanId = membershipPlanId;
            request.Amount = GetSelectedPlan()?.Price ?? 0;
        }

        private void OnPaymentMethodValueChanged(int paymentMethodId)
        {
            request.PaymentMethodId = paymentMethodId;
        }

        private int DaysRemaining(MemberShipModel membership)
        {
            return (membership.EndDate.ToDateTime(TimeOnly.MinValue) - MyanmarDateTimeFormatter.TodayMyanmarDateOnly.ToDateTime(TimeOnly.MinValue)).Days;
        }

        private bool IsExpiringSoon(MemberShipModel membership)
        {
            var days = DaysRemaining(membership);
            return membership.Status != "Expired" && days is >= 0 and <= 7;
        }

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "Active" => Color.Success,
                "Pending" => Color.Warning,
                _ => Color.Default
            };
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
