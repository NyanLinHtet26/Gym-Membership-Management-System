using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.DashBoard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.DashBoard
{
    public class DashBoardService
    {
        private readonly AppDbContext _db;

        public DashBoardService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<DashboardResponseModel> GetDashboardAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var sixMonthsAgo = firstDayOfMonth.AddMonths(-5);

            DashboardResponseModel model = new();

            // Cards
            model.TotalMembers = await _db.TblMembers.CountAsync();

            model.TodayIncome = await _db.TblPayments
                .Where(x =>
                    x.Status == "Paid" &&
                    x.CreatedAt.Date == today)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            model.MonthlyRevenue = await _db.TblPayments
                .Where(x =>
                    x.Status == "Paid" &&
                    x.CreatedAt >= firstDayOfMonth)
                .SumAsync(x => (decimal?)x.Amount) ?? 0;

            // Member Growth (Last 6 Months)
            model.MemberGrowths = await _db.TblMembers
                 .Where(x => x.CreatedAt >= sixMonthsAgo)
                 .GroupBy(x => new
                   {
                      x.CreatedAt.Year,
                      x.CreatedAt.Month
                   })
                 .Select(g => new MemberGrowthModel
                    {
                      Month = g.Key.Year + "-" + g.Key.Month,
                      TotalMembers = g.Count()
                    })
                .ToListAsync();

            // Revenue Trend (Last 6 Months)
            model.RevenueTrends = await _db.TblPayments
                 .Where(x => x.Status == "Paid" &&
                         x.CreatedAt >= sixMonthsAgo)
                 .GroupBy(x => new
                     {
                          x.CreatedAt.Year,
                          x.CreatedAt.Month
                     })
                 .Select(g => new RevenueTrendModel
                     {
                           Month = g.Key.Year + "-" + g.Key.Month,
                           Revenue = g.Sum(x => x.Amount)
                     })
                 .ToListAsync();

            // Most Used Plans
            model.MostUsedPlans = await _db.TblMemberships
                .GroupBy(x => x.MembershipPlan.PlanName)
                .Select(g => new PlanUsageModel
                {
                    PlanName = g.Key,
                    TotalMembers = g.Count()
                })
                .OrderByDescending(x => x.TotalMembers)
                .ToListAsync();

            return model;
        }
    }
}
