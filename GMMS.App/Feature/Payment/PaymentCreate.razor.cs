using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.MemberShip.Models;
using GMMS.Domain.Features.Payment.Models;
using GMMS.Domain.Features.PaymentMethod.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Payment
{
    public partial class PaymentCreate : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = null!;

        private CreatePaymentRequestModel request = new();
        private List<MemberShipModel> memberships = new();
        private List<PaymentMethodModel> paymentMethods = new();
        private bool isLoadingData = true;
        private bool isSaving;
        private bool loadFailed;
        private string? errorMessage;

        private MemberShipModel? SelectedMembership =>
            memberships.FirstOrDefault(m => m.MembershipId == request.MembershipId);

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            isLoadingData = true;
            loadFailed = false;
            errorMessage = null;

            try
            {
                var membershipResult = await ApiService.GetAllMembershipsAsync<Result<MemberShipListResponseModel>>(1, 100);
                if (membershipResult?.IsSuccess == true && membershipResult.Data is not null)
                    memberships = membershipResult.Data.MemberShips ?? new();
                else
                {
                    errorMessage = membershipResult?.Message ?? "Failed to load memberships.";
                    loadFailed = true;
                }

                var methodResult = await ApiService.GetPaymentMethodListAsync<Result<PaymentMethodListResponseModel>>(1, 100);
                if (methodResult?.IsSuccess == true && methodResult.Data is not null)
                    paymentMethods = (methodResult.Data.PaymentMethods ?? new()).Where(p => p.IsActive).ToList();
                else if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = methodResult?.Message ?? "Failed to load payment methods.";
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
                isLoadingData = false;
            }
        }

        private async Task RetryAsync() => await LoadDataAsync();

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

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Save()
        {
            if (request.MembershipId <= 0)
            {
                errorMessage = "Please select a membership.";
                return;
            }
            if (request.PaymentMethodId <= 0)
            {
                errorMessage = "Please select a payment method.";
                return;
            }
            if (request.Amount <= 0)
            {
                errorMessage = "Amount must be greater than zero.";
                return;
            }

            isSaving = true;
            errorMessage = null;

            try
            {
                var result = await ApiService.CreatePaymentAsync<CreatePaymentRequestModel, Result<PaymentDetailModel>>(request);

                if (result?.IsSuccess == true)
                {
                    Snackbar.Add("Payment created successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to create payment.";
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
