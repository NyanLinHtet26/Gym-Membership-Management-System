using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Enums;
using GMMS.Domain.Features.DashBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace GMMS.Domain.Features.DashBoard
{
    public class DashBoardService
    {
        private const int ChartMonthCount = 6;
        private const int TopPlanCount = 8;

        private readonly AppDbContext _db;

        public DashBoardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Result<DashboardResponseModel>> GetDashboardAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var chartStart = firstDayOfMonth.AddMonths(-(ChartMonthCount - 1));
            var completedStatus = PaymentStatus.Completed.ToString();

            DashboardResponseModel model = new();

            // Cards
            model.TotalMembers = await _db.TblMembers
                .CountAsync(x => !x.IsDeleted);

            model.TodayIncome = await _db.TblPayments
                .Where(x => !x.IsDeleted &&
                    x.Status == completedStatus &&
                    x.CreatedAt.Date == today)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            model.MonthlyRevenue = await _db.TblPayments
                .Where(x => !x.IsDeleted &&
                    x.Status == completedStatus &&
                    x.CreatedAt >= firstDayOfMonth)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            var months = GetMonthRange(chartStart, today);

            // Member Growth (continuous last 6 months)
            var growthRaw = await _db.TblMembers
                .Where(x => !x.IsDeleted && x.CreatedAt >= chartStart)
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var growthLookup = growthRaw.ToDictionary(x => (x.Year, x.Month), x => x.Count);

            model.MemberGrowths = months
                .Select(m => new MemberGrowthModel
                {
                    Month = FormatMonth(m),
                    TotalMembers = growthLookup.GetValueOrDefault(m)
                })
                .ToList();

            // Revenue Trend (continuous last 6 months)
            var revenueRaw = await _db.TblPayments
                .Where(x => !x.IsDeleted &&
                    x.Status == completedStatus &&
                    x.CreatedAt >= chartStart)
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Amount) })
                .ToListAsync();

            var revenueLookup = revenueRaw.ToDictionary(x => (x.Year, x.Month), x => x.Revenue);

            model.RevenueTrends = months
                .Select(m => new RevenueTrendModel
                {
                    Month = FormatMonth(m),
                    Revenue = revenueLookup.GetValueOrDefault(m)
                })
                .ToList();

            // Most Used Plans (top 8)
            model.MostUsedPlans = await _db.TblMemberships
                .Where(x => !x.IsDeleted && !x.MembershipPlan.IsDeleted)
                .GroupBy(x => x.MembershipPlan.PlanName)
                .Select(g => new PlanUsageModel
                {
                    PlanName = g.Key,
                    TotalMembers = g.Count()
                })
                .OrderByDescending(x => x.TotalMembers)
                .Take(TopPlanCount)
                .ToListAsync();

            return new Result<DashboardResponseModel>
            {
                IsSuccess = true,
                Message = "Dashboard data retrieved successfully.",
                Data = model,
                StatusCode = 200
            };
        }

        private static List<(int Year, int Month)> GetMonthRange(DateTime start, DateTime end)
        {
            var months = new List<(int Year, int Month)>();
            var current = new DateTime(start.Year, start.Month, 1);
            var last = new DateTime(end.Year, end.Month, 1);

            while (current <= last)
            {
                months.Add((current.Year, current.Month));
                current = current.AddMonths(1);
            }

            return months;
        }

        private static string FormatMonth((int Year, int Month) month)
        {
            return month.Year.ToString("D4") + "-" + month.Month.ToString("D2");
        }
    }
}
