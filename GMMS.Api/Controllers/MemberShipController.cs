using GMMS.Domain.Features.MemberShip;
using GMMS.Domain.Features.MemberShip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberShipController : BaseController
    {
        private readonly MemberShipService _memberShipService;
        private readonly ILogger<MemberShipController> _logger;

        public MemberShipController(MemberShipService memberShipService, ILogger<MemberShipController> logger)
        {
            _memberShipService = memberShipService;
            _logger = logger;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Memberlist([FromQuery] MemberShipListRequestModel request)
        {
            _logger.LogInformation("MemberList API called. MemberId={MemberId}, Page={PageNumber}, PageSize={PageSize}", request.MemberId, request.PageNumber, request.PageSize);

            if (request.MemberId <= 0)
            {
                _logger.LogWarning("MemberList API failed. MemberId is required.");
                return BadRequest("MemberId is required.");
            }

            var result = await _memberShipService.GetList(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("MemberList API successful. Total memberships fetched: {Count}", result.Data?.MemberShips?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("MemberList API failed. Message: {Message}", result.Message);
            }
            return Execute(result);
        }
        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllMemberships([FromQuery] AllMemberShipListRequestModel request)
        {
            _logger.LogInformation("GetAllMemberships API called. Page={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}, Status={Status}", request.PageNumber, request.PageSize, request.SearchTerm, request.Status);

            var result = await _memberShipService.GetAllList(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetAllMemberships API successful. Total memberships fetched: {Count}", result.Data?.MemberShips?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("GetAllMemberships API failed. Message: {Message}", result.Message);
            }
            return Execute(result);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberShipById([FromRoute] int id)
        {
            _logger.LogInformation("GetMemberShipById API called. MembershipId={MembershipId}", id);

            var result = await _memberShipService.GetById(id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetMemberShipById API completed successfully. MembershipId={MembershipId}", id);
            }
            else
            {
                _logger.LogWarning("GetMemberShipById API failed. MembershipId={MembershipId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMemberShip([FromBody] CreateMemberShipRequestModel request)
        {
            _logger.LogInformation("CreateMemberShip API called. MemberId={MemberId}, PlanId={PlanId}, Amount={Amount}", request.MemberId, request.MembershipPlanId, request.Amount);

            var result = await _memberShipService.Create(GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("CreateMemberShip API completed successfully. MemberId={MemberId}", request.MemberId);
            }
            else
            {
                _logger.LogWarning("CreateMemberShip API failed. MemberId={MemberId}, Message={Message}", request.MemberId, result.Message);
            }
            return Execute(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemberShip([FromRoute] int id, [FromBody] UpdateMembershipRequestModel request)
        {
            _logger.LogInformation("UpdateMemberShip API called. MembershipId={MembershipId}, PlanId={PlanId}", id, request.MembershipPlanId);

            if (id != request.MembershipId)
            {
                _logger.LogWarning("Route ID does not match request body ID. RouteId={RouteId}, BodyId={BodyId}", id, request.MembershipId);
                return BadRequest("Membership ID in the route does not match the ID in the request body.");
            }
            var result = await _memberShipService.Update(GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("UpdateMemberShip API completed successfully. MembershipId={MembershipId}", id);
            }
            else
            {
                _logger.LogWarning("UpdateMemberShip API failed. MembershipId={MembershipId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemberShip([FromRoute] int id)
        {
            _logger.LogInformation("DeleteMemberShip API called. MembershipId={MembershipId}", id);

            var result = await _memberShipService.Delete(id, GetCurrentUserId());
            if (result.IsSuccess)
            {
                _logger.LogInformation("DeleteMemberShip API completed successfully. MembershipId={MembershipId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteMemberShip API failed. MembershipId={MembershipId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
    }
}
