using GMMS.App.Services;
using GMMS.Domain;
using GMMS.Domain.Features.MemberShip.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Feature.Membership
{
    public partial class MembershipListAll : ComponentBase, IDisposable
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        [Inject]
        private AuthTokenStore AuthTokenStore { get; set; } = null!;

        [SupplyParameterFromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        private List<MemberShipModel>? memberships;
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

        private string? _selectedStatus;
        private string? selectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (_selectedStatus == value) return;
                _selectedStatus = value;
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
            if (memberships is null || Page != pageNumber)
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
                var result = await ApiService.GetAllMembershipsAsync<Result<MemberShipListResponseModel>>(pageNumber, pageSize, _searchTerm, _selectedStatus);
                if (result?.IsSuccess == true && result.Data is not null)
                {
                    memberships = result.Data.MemberShips;
                    totalCount = result.Data.TotalCount;
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                    if (totalPages < 1) totalPages = 1;
                }
                else
                {
                    errorMessage = result?.Message ?? "Failed to load memberships.";
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

        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "Active" => Color.Success,
                "Pending" => Color.Warning,
                _ => Color.Default
            };
        }

        private async Task RetryAsync() => await LoadPage(pageNumber);

        private int DaysRemaining(MemberShipModel membership)
        {
            return (membership.EndDate.ToDateTime(TimeOnly.MinValue) - MyanmarDateTimeFormatter.TodayMyanmarDateOnly.ToDateTime(TimeOnly.MinValue)).Days;
        }

        private bool IsExpiringSoon(MemberShipModel membership)
        {
            var days = DaysRemaining(membership);
            return membership.Status != "Expired" && days is >= 0 and <= 7;
        }

        private string? GetEndNote(MemberShipModel membership)
        {
            if (membership.Status == "Expired") return null;
            var days = DaysRemaining(membership);
            if (days < 0)
            {
                return $"{(membership.Status == "Expired" ? "Expired" : "Overdue")} {-days}d ago";
            }
            if (days <= 7)
            {
                return days == 0 ? "Expires today" : $"{days}d left";
            }
            return null;
        }

        private string GetEndNoteClass(MemberShipModel membership)
        {
            if (IsExpiringSoon(membership)) return "gmm-ms__soon";
            if (DaysRemaining(membership) < 0) return "gmm-ms__overdue";
            return "gmm-ms__remaining";
        }

        private string GetEndIcon(MemberShipModel membership)
        {
            if (membership.Status == "Expired") return Icons.Material.Filled.EventAvailable;
            if (IsExpiringSoon(membership)) return Icons.Material.Filled.WarningAmber;
            if (DaysRemaining(membership) < 0) return Icons.Material.Filled.Error;
            return Icons.Material.Filled.EventAvailable;
        }

        private string GetEndIconClass(MemberShipModel membership)
        {
            if (membership.Status == "Expired") return "gmm-ms__icon-ok";
            if (IsExpiringSoon(membership)) return "gmm-ms__icon-warn";
            if (DaysRemaining(membership) < 0) return "gmm-ms__icon-error";
            return "gmm-ms__icon-ok";
        }

        private async Task OpenCreateDialog()
        {
            var dialog = await DialogService.ShowAsync<MembershipCreate>("Create Membership");
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(1);
            }
        }

        private async Task OpenDetailDialog(int membershipId)
        {
            var parameters = new DialogParameters<MembershipDetail> { { x => x.MembershipId, membershipId } };
            await DialogService.ShowAsync<MembershipDetail>("Membership Details", parameters);
        }

        private async Task OpenEditDialog(int membershipId)
        {
            var parameters = new DialogParameters<MembershipEdit>
            {
                { x => x.MembershipId, membershipId }
            };
            var dialog = await DialogService.ShowAsync<MembershipEdit>("Edit Membership", parameters);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadPage(pageNumber);
            }
        }

        private async Task OpenDeleteDialog(int membershipId)
        {
            var parameters = new DialogParameters<MembershipDelete> { { x => x.MembershipId, membershipId } };
            var dialog = await DialogService.ShowAsync<MembershipDelete>("Delete Membership", parameters);
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
