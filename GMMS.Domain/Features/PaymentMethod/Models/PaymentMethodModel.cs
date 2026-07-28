using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.PaymentMethod.Models
{
    public class PaymentMethodListRequestModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }

    }
    public class PaymentMethodListResponseModel
    {
        public int TotalCount { get; set; }
        public List<PaymentMethodModel> PaymentMethods { get; set; }
    }
    public class PaymentMethodModel
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedByUser { get; set; }
    }
    public class PaymentMethodCreateRequestModel
    {
        public string PaymentMethodCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
    }
    public class PaymentMethodUpdateRequestModel
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethodCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
