using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.Member.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Member
{
    public partial class MemberList : ComponentBase, IDisposable
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [Inject]
        private NavigationManager Navigation { get; set; } = null!;

        [SupplyParameterFromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        [SupplyParameterFromQuery(Name = "search")]
        public string? Search { get; set; }

        private List<MemberModel>? members;
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
                    SyncUrl(1);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Page < 1) Page = 1;

            var requestedSearch = Search ?? "";
            if (members is null || Page != _pageSelected || requestedSearch != (_searchTerm ?? ""))
            {
                _searchTerm = requestedSearch;
                _pageSelected = Page;
                await LoadPage(Page);
            }
        }

        private async Task PageChanged(int page)
        {
            await LoadPage(page);
            SyncUrl(page);
        }

        private async Task RetryAsync()
        {
            await LoadPage(_pageSelected);
        }

        private void SyncUrl(int page)
        {
            Navigation.NavigateTo($"/member-list?page={page}&search={Uri.EscapeDataString(_searchTerm ?? "")}", replace: true);
        }

        private async Task LoadPage(int page, bool showLoading = true)
        {
            errorMessage = null;
            _pageSelected = page;

            if (showLoading)
            {
                isLoading = true;
            }

            try
            {
                var result = await ApiService.GetMemberListAsync<Result<MemberListResponseModel>>(_pageSelected, pageSize, _searchTerm);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    members = result.Data.Members;
                    totalCount = result.Data.TotalCount;
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                    if (totalPages < 1) totalPages = 1;
                    if (_pageSelected > totalPages) _pageSelected = totalPages;
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to load members.";
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
            var dialog = await DialogService.ShowAsync<MemberCreate>("Create Member");
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(1);
                SyncUrl(1);
            }
        }

        private async Task OpenEditDialog(int memberId)
        {
            var parameters = new DialogParameters<MemberEdit> { { x => x.MemberId, memberId } };
            var dialog = await DialogService.ShowAsync<MemberEdit>("Edit Member", parameters);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(_pageSelected);
            }
        }

        private async Task OpenDeleteDialog(int memberId)
        {
            var parameters = new DialogParameters<MemberDelete> { { x => x.MemberId, memberId } };
            var dialog = await DialogService.ShowAsync<MemberDelete>("Delete Member", parameters);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(_pageSelected);
            }
        }

        public void Dispose()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
        }
    }
}
