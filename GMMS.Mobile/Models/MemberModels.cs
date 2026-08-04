namespace GMMS.Mobile.Models;

public sealed class MemberListData
{
    public int TotalCount { get; set; }
    public List<Member> Members { get; set; } = new();
}

public sealed class Member
{
    public int MemberId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUser { get; set; }
}
