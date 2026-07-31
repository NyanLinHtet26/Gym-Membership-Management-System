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
        private string? errorMessage;

        private string _membershipStr
        {
            get => request.MembershipId > 0 ? request.MembershipId.ToString() : "";
            set => request.MembershipId = int.TryParse(value, out var id) ? id : 0;
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
                var membershipResult = await ApiService.GetAllMembershipsAsync<Result<MemberShipListResponseModel>>(1, 100);
                if (membershipResult?.IsSuccess == true && membershipResult.Data is not null)
                    memberships = membershipResult.Data.MemberShips ?? new();
                else
                    errorMessage = membershipResult?.Message ?? "Failed to load memberships.";

                var methodResult = await ApiService.GetPaymentMethodListAsync<Result<PaymentMethodListResponseModel>>(1, 100);
                if (methodResult?.IsSuccess == true && methodResult.Data is not null)
                    paymentMethods = (methodResult.Data.PaymentMethods ?? new()).Where(p => p.IsActive).ToList();
                else if (string.IsNullOrEmpty(errorMessage))
                    errorMessage = methodResult?.Message ?? "Failed to load payment methods.";
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
