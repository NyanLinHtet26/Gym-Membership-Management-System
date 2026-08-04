using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.MemberShip.Models
{
    public class MemberShipListRequestModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int MemberId { get; set; }
    }

    public class AllMemberShipListRequestModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public DateOnly? StartDateFrom { get; set; }
        public DateOnly? StartDateTo { get; set; }
        public DateOnly? EndDateFrom { get; set; }
        public DateOnly? EndDateTo { get; set; }
    }

    public class MemberShipListResponseModel
    {
        public int TotalCount { get; set; }
        public List<MemberShipModel> MemberShips { get; set; } = new();
    }
    public class MemberShipModel
    {
        public int MembershipId { get; set; }
        public string MemberCode { get; set; } = null!;
        public string MemberName { get; set; } = null!;
        public string PlanCode { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } = null!;
        public string? CreatedByUser { get; set; }
        public string? UpdatedByUser { get; set; }
    }
    public class MembershipDetailModel
    {
        public int MembershipId { get; set; }

        public int MemberId { get; set; }
        public string MemberCode { get; set; } = null!;
        public string MemberName { get; set; } = null!;

        public int MembershipPlanId { get; set; }
        public string PlanCode { get; set; } = null!;
        public string PlanName { get; set; } = null!;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public string? CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUser { get; set; }

    }
    public class CreateMemberShipRequestModel
    {
        public int MemberId { get; set; }
        public int MembershipPlanId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? Sspath { get; set; }
    }
    public class UpdateMembershipRequestModel
    {
        public int MembershipId { get; set; }
        public int MembershipPlanId { get; set; }
    }
}
