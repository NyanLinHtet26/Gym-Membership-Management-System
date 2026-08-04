using GMMS.Mobile.Models;

namespace GMMS.Mobile.Services;

public sealed class MemberApiService : ApiServiceBase
{
    public MemberApiService(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<MemberListData?> GetMemberListAsync(int pageNumber = 1, int pageSize = 20, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var uri = $"/api/Member?pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            uri += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        }

        return GetAsync<MemberListData>(uri, cancellationToken);
    }

    public Task<Member?> GetMemberAsync(int memberId, CancellationToken cancellationToken = default)
        => GetAsync<Member>($"/api/Member/{memberId}", cancellationToken);
}
