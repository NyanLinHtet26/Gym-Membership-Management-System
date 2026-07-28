using GMMS.Domain.Features.Auth;
using GMMS.Domain.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GMMS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly AuthService _authService;
        private readonly CookieService _cookieService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger,CookieService cookieService)
        {
            _authService = authService;
            _logger = logger;
            _cookieService = cookieService;
        }

        [HttpPost("login")]
        public async Task <IActionResult> Login([FromBody] LoginRequestModel request)
        {
            _logger.LogInformation("Login API called. UserName={UserName}", request.UserName);

            var result = await _authService.Login(request);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login APT failed. Username = {UserName} ",request.UserName);
                return Execute(result);
            }
            _cookieService.SetAuthCookies(Response, result.Data!.Tokens);

            _logger.LogInformation("AccessToken and RefreshToken cookies created");
            //=>return Execute(result);

            return Ok(new
            {
                isSuccess = true,
                message = "Login successful.",
                data = result.Data.User
            });




        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            _logger.LogInformation("Refresh token API called.");


            var refreshToken = Request.Cookies["refreshToken"];


            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token cookie missing.");

                return Unauthorized(new
                {
                    isSuccess = false,
                    message = "Refresh token is missing."
                });
            }


            var result = await _authService.RefreshToken(refreshToken);


            if (!result.IsSuccess)
            {
                _logger.LogWarning(
                    "Refresh token failed. Message={Message}",
                    result.Message
                );

                return Unauthorized(result);
            }


            _cookieService.SetAuthCookies(Response, result.Data!.Tokens);


            _logger.LogInformation("Token refreshed successfully. UserId={UserId}",result.Data.User.UserId);


            return Ok(new
            {
                isSuccess = true,
                message = "Token refreshed successfully.",
                data = result.Data.User
            });
        }


        [Authorize]
        [HttpPost("change-password")]
        public async Task <IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("ChangePassword API called. UserId={UserId}", userId);

            var result = await _authService.ChangePassword(userId, request);
            if (result.IsSuccess)
            {
                _logger.LogInformation("ChangePassword API completed successfully. UserId={UserId}", userId);
            }
            else
            {
                _logger.LogWarning("ChangePassword API failed. UserId={UserId}, Message={Message}", userId, result.Message);
            }
            return Execute(result);
        }
    }
}
