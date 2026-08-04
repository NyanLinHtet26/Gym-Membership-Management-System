using GMMS.Domain.Features.Auth;
using GMMS.Api.Extensions;
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

            return Ok(new
            {
                isSuccess = true,
                message = "Login successful.",
                data = new
                {
                    user = new
                    {
                        result.Data.User.UserId,
                        result.Data.User.UserName,
                        result.Data.User.Role,
                        result.Data.User.MustChangePassword
                    },
                    accessToken = result.Data.Tokens.AccessToken.Token,
                    refreshToken = new
                    {
                        result.Data.Tokens.RefreshToken.Token,
                        result.Data.Tokens.RefreshToken.ExpiresAt
                    }
                }
            });

        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestModel? request = null)
        {
            _logger.LogInformation("Refresh token API called.");

            var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token missing.");

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
                data = new
                {
                    user = new
                    {
                        result.Data.User.UserId,
                        result.Data.User.UserName,
                        result.Data.User.Role,
                        result.Data.User.MustChangePassword
                    },
                    accessToken = result.Data.Tokens.AccessToken.Token,
                    refreshToken = new
                    {
                        result.Data.Tokens.RefreshToken.Token,
                        result.Data.Tokens.RefreshToken.ExpiresAt
                    }
                }
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
                _cookieService.ClearAuthCoookies(Response);
                _logger.LogInformation("ChangePassword API completed successfully. UserId={UserId}. Sessions revoked, cookies cleared.", userId);
            }
            else
            {
                _logger.LogWarning("ChangePassword API failed. UserId={UserId}, Message={Message}", userId, result.Message);
            }
            return Execute(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var sessionIdClaim = User.FindFirst("SessionId")?.Value;

            if (string.IsNullOrEmpty(sessionIdClaim))
            {
                return Unauthorized(new
                {
                    isSuccess = false,
                    message = "Session information missing."
                });
            }


            if (!Guid.TryParse(sessionIdClaim, out var sessionId))
            {
                return Unauthorized(new
                {
                    isSuccess = false,
                    message = "Invalid session."
                });
            }


            var result = await _authService.Logout(sessionId);


            if (result.IsSuccess)
            {
                _cookieService.ClearAuthCoookies(Response);
            }


            return Execute(result);
        }
    }
}
