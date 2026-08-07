using GMMS.Domain;
using GMMS.Domain.Features.Member;
using GMMS.Domain.Features.Member.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : BaseController
    {
        private readonly MemberService _memberService;
        private readonly ILogger<MemberController> _logger;

        public MemberController(MemberService memberService, ILogger<MemberController> logger)
        {
            _memberService = memberService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> MemberList([FromQuery] MemberListRequestModel request)
        {
            _logger.LogInformation("MemberList API called. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);
            var result = await _memberService.GetList(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("MemberList API successful. Total members fetched: {Count}", result.Data?.Members?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("MemberList API failed. Message: {Message}", result.Message);
            }
            return Execute(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberById([FromRoute] int id)
        {
            _logger.LogInformation("GetMemberById API called. MemberId: {MemberId}", id);
            var result = await _memberService.GetById(id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetMemberById API completed successfully. MemberId: {MemberId}", id);
            }
            else
            {
                _logger.LogWarning("GetMemberById API failed. MemberId: {MemberId}, Message: {Message}", id, result.Message);
            }
            return Execute(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMember([FromBody] CreateMemberRequestModel request)
        {
            _logger.LogInformation("CreateMember API called. MemberCode: {MemberCode}, Name: {Name}", request.MemberCode, request.Name);
            var result = await _memberService.Create(GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("CreateMember API completed successfully. MemberCode: {MemberCode}, Name: {Name}", request.MemberCode, request.Name);
            }
            else
            {
                _logger.LogWarning("CreateMember API failed. MemberCode: {MemberCode}, Message: {Message}", request.MemberCode, result.Message);
            }
            return Execute(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember([FromRoute] int id, [FromBody] UpdateMemberRequestModel request)
        {
            _logger.LogInformation("UpdateMember API called. MemberId: {MemberId}, MemberCode: {MemberCode}", id, request.MemberCode);
            if (id != request.MemberId)
            {
                _logger.LogWarning("Route ID does not match request body ID. RouteId: {RouteId}, BodyId: {BodyId}", id, request.MemberId);
                return Execute(new Result<MemberModel>
                {
                    IsSuccess = false,
                    Message = "Member ID in the route does not match the ID in the request body.",
                    StatusCode = 400
                });
            }
            var result = await _memberService.Update(id, GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("UpdateMember API completed successfully. MemberId: {MemberId}", result.Data?.MemberId);
            }
            else
            {
                _logger.LogWarning("UpdateMember API failed. MemberId: {MemberId}, Message: {Message}", id, result.Message);
            }
            return Execute(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember([FromRoute] int id)
        {
            _logger.LogInformation("DeleteMember API called. MemberId: {MemberId}", id);
            var result = await _memberService.Delete(id, GetCurrentUserId());
            if (result.IsSuccess)
            {
                _logger.LogInformation("DeleteMember API completed successfully. MemberId: {MemberId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteMember API failed. MemberId: {MemberId}, Message: {Message}", id, result.Message);
            }
            return Execute(result);
        }
    }
}
