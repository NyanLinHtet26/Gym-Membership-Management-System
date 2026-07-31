using GMMS.Domain.Features.User;
using GMMS.Domain.Features.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]    
    public class UserController : BaseController
    {
        private readonly UserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task <IActionResult> UserList([FromQuery] UserListRequestModel request)
        {
            _logger.LogInformation("UserList API called. PageNumber={PageNumber}, PageSize={PageSize}, SearchTerm={SearchTerm}", request.PageNumber, request.PageSize, request.SearchTerm);

            var result = await _userService.GetList(request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("UserList API successful. TotalCount={TotalCount}", result.Data?.TotalCount ?? 0);
            }
            else
            {
                _logger.LogWarning("UserList API failed. Message={Message}", result.Message);
            }
            return Execute(result);
        }

        [HttpGet("{id}")]
        public async Task <IActionResult> GetUser([FromRoute] int id)
        {
            _logger.LogInformation("GetUser API called. UserId={UserId}", id);

            var result =await _userService.GetById(id);
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetUser API completed successfully. UserId={UserId}", id);
            }
            else
            {
                _logger.LogWarning("GetUser API failed. UserId={UserId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }

        [HttpPost]
        public async Task <IActionResult> CreateUser([FromBody] CreateUserRequestModel request)
        {
            _logger.LogInformation("CreateUser API called. UserName={UserName}, Role={Role}", request.UserName, request.Role);

            var result = await _userService.Create(GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("CreateUser API completed successfully. UserName={UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("CreateUser API failed. Message={Message}", result.Message);
            }
            return Execute(result);
        }

        [HttpPut("{id}")]
        public async Task <IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequestModel request)
        {
            _logger.LogInformation("UpdateUser API called. UserId={UserId}, UserName={UserName}", id, request.UserName);

            if (id != request.UserId)
            {
                _logger.LogWarning("Route ID does not match request body ID. RouteId={RouteId}, BodyId={BodyId}", id, request.UserId);
                return BadRequest("User ID in the route does not match the ID in the request body.");
            }
            var result = await _userService.Update(id, GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("UpdateUser API completed successfully. UserId={UserId}", id);
            }
            else
            {
                _logger.LogWarning("UpdateUser API failed. UserId={UserId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }

        [HttpPost("reset-password")]
        public async Task <IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel request)
        {
            _logger.LogInformation("ResetPassword API called. UserId={UserId}", request.UserId);

            var result =await _userService.ResetPassword(GetCurrentUserId(), request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("ResetPassword API completed successfully. UserId={UserId}", request.UserId);
            }
            else
            {
                _logger.LogWarning("ResetPassword API failed. UserId={UserId}, Message={Message}", request.UserId, result.Message);
            }
            return Execute(result);
        }

        [HttpDelete("{id}")]
        public async Task <IActionResult> DeleteUser([FromRoute] int id)
        {
            _logger.LogInformation("DeleteUser API called. UserId={UserId}", id);

            var result = await _userService.Delete(id, GetCurrentUserId());
            if (result.IsSuccess)
            {
                _logger.LogInformation("DeleteUser API completed successfully. UserId={UserId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteUser API failed. UserId={UserId}, Message={Message}", id, result.Message);
            }
            return Execute(result);
        }
    }
}
