using FluentValidation;
using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.Auth.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GMMS.Domain.Features.Auth;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IValidator<LoginRequestModel> _loginValidator;
    private readonly IValidator<ChangePasswordRequestModel> _changePasswordValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IConfiguration configuration,
        IValidator<LoginRequestModel> loginValidator,
        IValidator<ChangePasswordRequestModel> changePasswordValidator,
        ILogger<AuthService> logger)
    {
        _db = db;
        _configuration = configuration;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _logger = logger;
    }

    public async Task <Result<LoginResultModel>> Login(LoginRequestModel request)
    {
        _logger.LogInformation("Login attempt for UserName: {UserName}", request.UserName);

        var validationResult =  await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid login request for UserName: {UserName}. Errors: {Errors}", request.UserName, string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return new Result<LoginResultModel>
            {
                IsSuccess = false,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        
            var user = await _db.TblUsers
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserName == request.UserName && x.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for UserName: {UserName} - invalid credentials.", request.UserName);
                return new Result<LoginResultModel>
                {
                    IsSuccess = false,
                    Message = "Invalid username or password."
                };
            }

        var sessionId = Guid.NewGuid();

        var tokenResult =  GenerateToken(user, sessionId);

        

            if (!tokenResult.IsSuccess)
            {
                _logger.LogWarning("Token generation failed for UserId: {UserId}. Message: {Message}", user.UserId, tokenResult.Message);
                return new Result<LoginResultModel>
                {
                    IsSuccess = false,
                    Message = tokenResult.Message
                };
            }

            var session = new TblUserSession
            {
                UserId = user.UserId,
                SessionId =  sessionId,
                LoginTime = DateTime.UtcNow,
                ExpiredAt = tokenResult.Data!.User.ExpiresAt,
                IsExpired = false
            };

            await _db.TblUserSessions.AddAsync(session);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Login successful for UserId: {UserId}, UserName: {UserName}", user.UserId, request.UserName);

            return new Result<LoginResultModel>
            {
                IsSuccess = true,
                Message = "Login successful.",
                Data = tokenResult.Data
            };
        
    }

    public async Task<Result<bool>> ChangePassword(int userId, ChangePasswordRequestModel request)
    {
        _logger.LogInformation("Change password attempt for UserId: {UserId}", userId);

        var validationResult = await _changePasswordValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid change password request for UserId: {UserId}. Errors: {Errors}", userId, string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return new Result<bool>
            {
                IsSuccess = false,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        
            var user = await _db.TblUsers
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.UserId == userId && x.IsActive);

            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found for password change.", userId);
                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = "User not found."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Incorrect current password for UserId: {UserId}.", userId);
                return new Result<bool>
                {
                    IsSuccess = false,
                    Message = "Current password is incorrect."
                };
            }

        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
        {
            return new Result<bool>
            {
                IsSuccess = false,
                Message = "New password cannot be the same as current password."
            };
        }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for UserId: {UserId}", userId);

            return new Result<bool>
            {
                IsSuccess = true,
                Message = "Password changed successfully.",
                Data = true
            };
        
       
    }

    private  Result<LoginResultModel> GenerateToken(TblUser user,Guid sessionId)
    {
       
            var jwtKey = _configuration["JwtSettings:Key"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];
            var jwtExpiryMinutes = 60;
                int.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out jwtExpiryMinutes);

            if (string.IsNullOrEmpty(jwtKey))
            {
            _logger.LogError("JWT Key is missing.");
            return new Result<LoginResultModel>
                {
                    IsSuccess = false,
                    Message = "JWT key is not configured."
                };
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(jwtExpiryMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("SessionId",sessionId.ToString()),
                new Claim("MustChangePassword", user.MustChangePassword.ToString().ToLower())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new Result<LoginResultModel>
            {
                IsSuccess = true,
                Data = new LoginResultModel
                {
                    AccessToken = tokenString,

                    User = new LoginResponseModel
                    {
                        UserId = user.UserId,
                        UserName = user.UserName,
                        Role = user.Role,
                        MustChangePassword = user.MustChangePassword,
                        ExpiresAt = expiresAt
                    }
                }
            };
       
    }
}
