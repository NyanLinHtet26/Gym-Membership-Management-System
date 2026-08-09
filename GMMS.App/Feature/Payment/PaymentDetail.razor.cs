using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Payment.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Payment
{
    public partial class PaymentDetail : ComponentBase
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter]
        public int PaymentId { get; set; }

        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        private PaymentDetailModel? detail;
        private bool isLoading = true;
        private bool loadFailed;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            isLoading = true;
            loadFailed = false;
            errorMessage = null;

            try
            {
                var result = await ApiService.GetPaymentDetailsAsync<Result<PaymentDetailModel>>(PaymentId);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    detail = result.Data;
                }
                else
                {
                    errorMessage = result?.Message ?? "Payment not found.";
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

        private async Task RetryAsync() => await LoadDataAsync();

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "Completed" => Color.Success,
                "Pending" => Color.Warning,
                "Failed" => Color.Error,
                _ => Color.Default
            };
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}
