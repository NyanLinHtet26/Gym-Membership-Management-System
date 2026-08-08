using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.DashBoard.Models
{
    public class DashboardResponseModel
    {
        // Cards
        public int TotalMembers { get; set; }

        public decimal TodayIncome { get; set; }

        public decimal MonthlyRevenue { get; set; }

        // Monthly Member Growth Chart
        public List<MemberGrowthModel> MemberGrowths { get; set; } = [];

        // Revenue Trend Chart
        public List<RevenueTrendModel> RevenueTrends { get; set; } = [];

        // Most Used Plans Chart
        public List<PlanUsageModel> MostUsedPlans { get; set; } = [];
    }

    public class RevenueTrendModel
    {
        public string Month { get; set; } = string.Empty;

        public decimal Revenue { get; set; }
    }

    public class MemberGrowthModel
    {
        public string Month { get; set; } = string.Empty;
        public int TotalMembers { get; set; }
    }

    public class PlanUsageModel
    {
        public string PlanName { get; set; } = string.Empty;
        public int TotalMembers { get; set; }
    }
}
