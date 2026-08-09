using GMMS.Domain.Features.MemberShipPlan;
using GMMS.Domain.Features.MemberShipPlan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberShipPlanController : BaseController
    {
        private readonly MemberShipPlanService _memberShipPlanService;
        private readonly ILogger<MemberShipPlanController> _logger;

        public MemberShipPlanController(MemberShipPlanService memberShipPlanService, ILogger<MemberShipPlanController> logger)
        {
            _memberShipPlanService = memberShipPlanService;
            _logger = logger;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MemberShipPlanList([FromQuery] MemberShipPlanlistRequestModel request)
        {
            _logger.LogInformation("MemberShipPlanList API called. Page={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            var result = await _memberShipPlanService.GetList(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("MemberShipPlanList API successful. Total plans fetched: {Count}", result.Data?.MemberShipPlans?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("MemberShipPlanList API failed. Message: {Message}", result.Message);
            }
            return Execute(result);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> MemberShipPlanById([FromRoute] int id)
        {
            _logger.LogInformation("MemberShipPlanById API called. MembershipPlanId={MembershipPlanId}", id);

            var result = await _memberShipPlanService.GetById(id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("MemberShipPlanById API completed successfully. MembershipPlanId={MembershipPlanId}", id);
            }
            else
            {
                _logger.LogWarning("MemberShipPlanById API failed. MembershipPlanId={MembershipPlanId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpPost]
        public async Task<IActionResult> CreateMemberShipPlan([FromBody] CreateMemberShipPlanRequestModel request)
        {
            _logger.LogInformation("CreateMemberShipPlan API called. PlanCode={PlanCode}, PlanName={PlanName}", request.PlanCode, request.PlanName);

            var result = await _memberShipPlanService.Create(GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("CreateMemberShipPlan API completed successfully. PlanCode={PlanCode}", request.PlanCode);
            }
            else
            {
                _logger.LogWarning("CreateMemberShipPlan API failed. Message={Message}", result.Message);
            }
            return Execute(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemberShipPlan([FromRoute] int id, [FromBody] UpdateMemberShipPlanRequestModel request)
        {
            _logger.LogInformation("UpdateMemberShipPlan API called. MembershipPlanId={MembershipPlanId}, PlanCode={PlanCode}", id, request.PlanCode);

            if (id != request.MemberShipPlanId)
            {
                _logger.LogWarning("Route ID does not match request body ID. RouteId={RouteId}, BodyId={BodyId}", id, request.MemberShipPlanId);
                return BadRequest("Member ID in the route does not match the ID in the request body.");
            }
            var result = await _memberShipPlanService.Update(id, GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("UpdateMemberShipPlan API completed successfully. MembershipPlanId={MembershipPlanId}", id);
            }
            else
            {
                _logger.LogWarning("UpdateMemberShipPlan API failed. MembershipPlanId={MembershipPlanId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
        [Authorize(Roles = "Owner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemberShipPlan([FromRoute] int id)
        {
            _logger.LogInformation("DeleteMemberShipPlan API called. MembershipPlanId={MembershipPlanId}", id);

            var result = await _memberShipPlanService.Delete(id, GetCurrentUserId());
            if (result.IsSuccess)
            {
                _logger.LogInformation("DeleteMemberShipPlan API completed successfully. MembershipPlanId={MembershipPlanId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteMemberShipPlan API failed. MembershipPlanId={MembershipPlanId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
    }
}
