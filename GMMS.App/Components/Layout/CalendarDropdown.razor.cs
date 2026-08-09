using GMMS.App.Feature.Membership;
using GMMS.App.Feature.Payment;
using GMMS.App.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GMMS.App.Components.Layout
{
    public partial class CalendarDropdown : ComponentBase
    {
        [Inject]
        private CalendarService CalendarService { get; set; } = null!;

        [Inject]
        private IDialogService DialogService { get; set; } = null!;

        private static readonly string[] _weekDayLabels = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        private readonly DateOnly _today = MyanmarDateTimeFormatter.TodayMyanmarDateOnly;

        private int _year;
        private int _month;
        private bool _showPayments = true;
        private bool _showStarts = true;
        private bool _showEnds = true;
        private DateOnly? _selectedDate;
        private List<DayEvent> _events = new();
        private List<DateOnly?> _days = new();

        private string MonthTitle => new DateTime(_year, _month, 1).ToString("MMMM yyyy");

        private List<DayEvent> FilteredEvents => _events
            .Where(e => (_showPayments && e.Type == "Payment")
                || (_showStarts && e.Type == "Start")
                || (_showEnds && e.Type == "End"))
            .ToList();

        protected override async Task OnInitializedAsync()
        {
            var today = MyanmarDateTimeFormatter.TodayMyanmarDateTime;
            _year = today.Year;
            _month = today.Month;
            BuildGrid();
            await LoadEventsAsync();
        }

        private void BuildGrid()
        {
            _days = new List<DateOnly?>();
            var firstOfMonth = new DateOnly(_year, _month, 1);
            var daysInMonth = DateTime.DaysInMonth(_year, _month);
            var leading = (int)firstOfMonth.DayOfWeek;

            for (var i = 0; i < leading; i++)
            {
                _days.Add(null);
            }

            for (var day = 1; day <= daysInMonth; day++)
            {
                _days.Add(new DateOnly(_year, _month, day));
            }

            while (_days.Count % 7 != 0)
            {
                _days.Add(null);
            }
        }

        private async Task LoadEventsAsync()
        {
            try
            {
                _events = await CalendarService.GetMonthEventsAsync(_year, _month) ?? new List<DayEvent>();
            }
            catch
            {
                _events = new List<DayEvent>();
            }
        }

        private async Task PrevMonth()
        {
            _month--;
            if (_month < 1)
            {
                _month = 12;
                _year--;
            }
            _selectedDate = null;
            BuildGrid();
            await LoadEventsAsync();
        }

        private async Task NextMonth()
        {
            _month++;
            if (_month > 12)
            {
                _month = 1;
                _year++;
            }
            _selectedDate = null;
            BuildGrid();
            await LoadEventsAsync();
        }

        private Task PaymentsChanged(bool value)
        {
            _showPayments = value;
            return Task.CompletedTask;
        }

        private Task StartsChanged(bool value)
        {
            _showStarts = value;
            return Task.CompletedTask;
        }

        private Task EndsChanged(bool value)
        {
            _showEnds = value;
            return Task.CompletedTask;
        }

        private void SelectDay(DateOnly date)
        {
            _selectedDate = date;
        }

        private async Task OpenDetail(DayEvent ev)
        {
            if (ev.Type == "Payment")
            {
                var parameters = new DialogParameters<PaymentDetail> { { x => x.PaymentId, ev.RefId } };
                await DialogService.ShowAsync<PaymentDetail>("Payment Details", parameters);
            }
            else
            {
                var parameters = new DialogParameters<MembershipDetail> { { x => x.MembershipId, ev.RefId } };
                await DialogService.ShowAsync<MembershipDetail>("Membership Details", parameters);
            }
        }
    }
}
