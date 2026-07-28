using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.Payment.Models
{
    public class PaymentListRequestModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
    public class PaymentListResponseModel
    {
        public int TotalCount { get; set; }
        public List<PaymentModel> Payments { get; set; }
    }
    public class PaymentModel
    {
        public int PaymentId { get; set; }
        public int MembershipId { get; set; }
        public string PaymentMethodName { get; set; } = null!;

        public decimal Amount { get; set; }

        public string? Sspath { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public string? CreatedByUser { get; set; }
    }
    public class PaymentDetailModel
    {
        public int PaymentId { get; set; }

        public string MemberName { get; set; } = null!;

        public string MembershipName { get; set; } = null!;

        public string PaymentMethodName { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public string? CreatedByUser { get; set; }
    }
    public class CreatePaymentRequestModel
    {
        public int MembershipId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string Sspath { get; set; } = null!;
    }
    
}
