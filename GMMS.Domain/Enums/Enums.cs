using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Enums
{
    public enum MembershipPlanStatus
    {
        none,
        Pending,
        Active,
        Expired
        
    }
    public enum PaymentStatus
    {
        none,
        Pending,
        Paid,
        Failed
    }

}
