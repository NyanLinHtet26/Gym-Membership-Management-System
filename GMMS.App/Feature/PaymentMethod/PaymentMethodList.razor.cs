using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.PaymentMethod.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.PaymentMethod
{
    public partial class PaymentMethodList : ComponentBase, IDisposable
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [SupplyParameterFromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        private List<PaymentMethodModel>? methods;
        private int pageNumber = 1;
        private int pageSize = 10;
        private int totalCount;
        private int totalPages;
        private bool isLoading;
        private string? errorMessage;

        private int _pageSelected = 1;
        private CancellationTokenSource? _searchCts;

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
            if (methods is null || Page != pageNumber)
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
                var result = await ApiService.GetPaymentMethodListAsync<Result<PaymentMethodListResponseModel>>(pageNumber, pageSize, _searchTerm);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    methods = result.Data.PaymentMethods;
                    totalCount = result.Data.TotalCount;
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                    if (totalPages < 1) totalPages = 1;
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to load payment methods.";
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

        private async Task OpenCreateDialog()
        {
            var dialog = await DialogService.ShowAsync<PaymentMethodCreate>("Create Payment Method");
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(1);
            }
        }

        private async Task OpenEditDialog(int paymentMethodId)
        {
            var parameters = new DialogParameters<PaymentMethodEdit> { { x => x.PaymentMethodId, paymentMethodId } };
            var dialog = await DialogService.ShowAsync<PaymentMethodEdit>("Edit Payment Method", parameters);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(pageNumber);
            }
        }

        private async Task OpenDeleteDialog(int paymentMethodId)
        {
            var parameters = new DialogParameters<PaymentMethodDelete> { { x => x.PaymentMethodId, paymentMethodId } };
            var dialog = await DialogService.ShowAsync<PaymentMethodDelete>("Delete Payment Method", parameters);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(pageNumber);
            }
        }

        public void Dispose()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
        }
    }
}
