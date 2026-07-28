using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.MemberShipPlan.Models
{
    public class MemberShipPlanlistRequestModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
    public class MemberShipPlanListResponseModel
    {
        public int TotalCount { get; set; }
        public List<MemberShipPlanModel> MemberShipPlans { get; set; }
    }
    public class MemberShipPlanModel
    {
        public int MemberShipPlanId { get; set; }
        public string PlanCode { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUser { get; set; }
    }
    public class MembershipPlanDetailModel
    {
        public int MemberShipPlanId { get; set; }
        public string PlanCode { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUser { get; set; }
    }


    public class CreateMemberShipPlanRequestModel
    {
        public string PlanCode { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }
    public class UpdateMemberShipPlanRequestModel
    {
        
        public int MemberShipPlanId { get; set; }
        public string PlanCode { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
