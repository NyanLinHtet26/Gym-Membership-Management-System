using GMMS.Domain;
using GMMS.Domain.Features.MemberShip.Models;
using GMMS.Domain.Features.Payment.Models;

namespace GMMS.App.Services
{
    public class DayEvent
    {
        public DateOnly Date { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Detail { get; set; } = null!;
        public int RefId { get; set; }
    }

    public class CalendarService
    {
        private readonly ApiService _api;

        public CalendarService(ApiService api)
        {
            _api = api;
        }

        public async Task<List<DayEvent>> GetMonthEventsAsync(int year, int month)
        {
            var events = new List<DayEvent>();
            var firstDay = new DateOnly(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var paymentResult = await _api.GetPaymentListAsync<Result<PaymentListResponseModel>>(
                1, 1000,
                fromDate: firstDay.ToDateTime(TimeOnly.MinValue),
                toDate: lastDay.ToDateTime(TimeOnly.MaxValue));

            if (paymentResult?.IsSuccess == true && paymentResult.Data?.Payments is not null)
            {
                foreach (var payment in paymentResult.Data.Payments)
                {
                    events.Add(new DayEvent
                    {
                        Date = DateOnly.FromDateTime(payment.CreatedAt),
                        Type = "Payment",
                        Title = $"Payment ({payment.Status})",
                        Detail = $"{CurrencyFormatter.FormatMMK(payment.Amount)} via {payment.PaymentMethodName}",
                        RefId = payment.PaymentId
                    });
                }
            }

            var membershipResult = await _api.GetAllMembershipsAsync<Result<MemberShipListResponseModel>>(
                1, 1000,
                startDateFrom: firstDay,
                startDateTo: lastDay,
                endDateFrom: firstDay,
                endDateTo: lastDay);

            if (membershipResult?.IsSuccess == true && membershipResult.Data?.MemberShips is not null)
            {
                foreach (var membership in membershipResult.Data.MemberShips)
                {
                    if (membership.StartDate >= firstDay && membership.StartDate <= lastDay)
                    {
                        events.Add(new DayEvent
                        {
                            Date = membership.StartDate,
                            Type = "Start",
                            Title = $"Start: {membership.MemberName}",
                            Detail = membership.PlanName,
                            RefId = membership.MembershipId
                        });
                    }

                    if (membership.EndDate >= firstDay && membership.EndDate <= lastDay)
                    {
                        events.Add(new DayEvent
                        {
                            Date = membership.EndDate,
                            Type = "End",
                            Title = $"End: {membership.MemberName}",
                            Detail = membership.PlanName,
                            RefId = membership.MembershipId
                        });
                    }
                }
            }

            return events;
        }
    }
}
