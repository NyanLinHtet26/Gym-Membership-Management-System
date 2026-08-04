using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.Member.Models;

public class MemberListRequestModel
{
    public int PageNumber { get; set; } 
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
}
public class MemberListResponseModel
{
    public int TotalCount { get; set; }
    public List<MemberModel> Members { get; set; } = new();
}
public class MemberModel
{
    public int MemberId { get; set; }
    public string MemberCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUser { get; set; }
}

public class CreateMemberRequestModel
{
    public string MemberCode { get; set; } = null!;
    public string Name { get; set; } = null!;
}
public class UpdateMemberRequestModel
{
    public int MemberId { get; set; }
    public string MemberCode { get; set; } = null!;
    public string Name { get; set; } = null!;
}

