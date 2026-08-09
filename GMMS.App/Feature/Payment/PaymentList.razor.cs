using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Payment.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Payment
{
    public partial class PaymentList : ComponentBase, IDisposable
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [SupplyParameterFromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        private List<PaymentModel>? payments;
        private int pageNumber = 1;
        private int pageSize = 10;
        private int totalCount;
        private int totalPages;
        private bool isLoading;
        private string? errorMessage;

        private int _pageSelected = 1;
        private CancellationTokenSource? _searchCts;

        private DateRange? _dateRange;
        public DateRange? SelectedDateRange
        {
            get => _dateRange;
            set
            {
                _dateRange = value;
                if (value is null || (value.Start.HasValue && value.End.HasValue))
                {
                    _ = LoadPage(1, false);
                }
            }
        }

        private string? _searchTerm;
        private string? searchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm == value) return;
                _searchTerm = value;
                DebounceSearch();
            }
        }

        private async void DebounceSearch()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                {
                    await LoadPage(1, false);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Page < 1) Page = 1;
            _pageSelected = Page;
            if (payments is null || Page != pageNumber)
            {
                await LoadPage(Page);
            }
        }

        private async Task PageChanged(int page)
        {
            _pageSelected = page;
            await LoadPage(page);
        }

        private async Task LoadPage(int page, bool showLoading = true)
        {
            errorMessage = null;
            pageNumber = page;

            if (showLoading)
            {
                isLoading = true;
            }

            try
            {
                var result = await ApiService.GetPaymentListAsync<Result<PaymentListResponseModel>>(
                    pageNumber, pageSize, _searchTerm, SelectedDateRange?.Start, SelectedDateRange?.End);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    payments = result.Data.Payments;
                    totalCount = result.Data.TotalCount;
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                    if (totalPages < 1) totalPages = 1;
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to load payments.";
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

        private async Task RetryAsync() => await LoadPage(pageNumber);

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

        private async Task OpenCreateDialog()
        {
            var dialog = await DialogService.ShowAsync<PaymentCreate>("Create Payment");
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(1);
            }
        }

        private async Task OpenDetailDialog(int paymentId)
        {
            var parameters = new DialogParameters<PaymentDetail> { { x => x.PaymentId, paymentId } };
            await DialogService.ShowAsync<PaymentDetail>("Payment Details", parameters);
        }

        public void Dispose()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
        }
    }
}
