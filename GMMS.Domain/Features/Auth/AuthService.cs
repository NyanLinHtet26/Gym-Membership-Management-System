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
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IValidator<LoginRequestModel> _loginValidator;
    private readonly IValidator<ChangePasswordRequestModel> _changePasswordValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IConfiguration configuration,
        IValidator<LoginRequestModel> loginValidator,
        IValidator<ChangePasswordRequestModel> changePasswordValidator,
        ILogger<AuthService> logger,
        TokenService tokenService)
        
    {
        _db = db;
        _configuration = configuration;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
        _logger = logger;
        _tokenService = tokenService;
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

        var tokens = _tokenService.CreateTokens(user, sessionId);

        var session = new TblUserSession
            {
                UserId = user.UserId,
                SessionId =  sessionId,
                LoginTime = DateTime.UtcNow,
                
                RefreshTokenHash = _tokenService.HashRefreshToken(tokens.RefreshToken.Token),
                RefreshTokenExpiresAt = tokens.RefreshToken.ExpiresAt,
                AccessTokenExpiresAt = tokens.AccessToken.ExpiresAt,
                


                IsExpired = false
            };

            await _db.TblUserSessions.AddAsync(session);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Login successful for UserId: {UserId}, UserName: {UserName}", user.UserId, request.UserName);


        var loginResult = new LoginResultModel
        {
            User = new LoginResponseModel
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Role = user.Role,
                MustChangePassword = user.MustChangePassword
            },

            Tokens = tokens
        };


        return new Result<LoginResultModel>
        {
            IsSuccess = true,
            Message = "Login successful.",
            Data = loginResult
        };

    }

    public async Task<Result<LoginResultModel>> RefreshToken(string refreshtoken)
    {
        if (string.IsNullOrWhiteSpace(refreshtoken))
        {
            return new Result<LoginResultModel>
            {
                IsSuccess = false,
                Message = "Referch token is missing"
            };
        }
        //find Sesion 
        var session = await _db.TblUserSessions
            .FirstOrDefaultAsync(x => !x.IsExpired && x.RefreshTokenExpiresAt > DateTime.UtcNow);

        if (session == null)
        {
            return new Result<LoginResultModel>
            {
                IsSuccess = false,
                Message = "Invalid refresh token."
            };
        }

        //Verify Refersh Token
        var isValid = _tokenService.VerifyRefreshToken(refreshtoken,session.RefreshTokenHash);

        if (!isValid)
        {
            return new Result<LoginResultModel>
            {
                IsSuccess = false,
                Message = "Invalid refresh token"
            };
        }

        //GetUser 
        var user = await _db.TblUsers
            .FirstOrDefaultAsync(x => x.UserId == session.UserId && !x.IsDeleted && x.IsActive);

        if (user == null)
        {
            return new Result<LoginResultModel>
            {
                IsSuccess = false,
                Message = "User not found"
            };
        }

        //Revoke old Session 
        session.IsExpired = true;
        session.RevokedAt = DateTime.UtcNow;

        //create new Tokens
        var newSessionId = Guid.NewGuid();
        var tokens = _tokenService.CreateTokens(user, newSessionId);

        //Save new session
        var newSession = new TblUserSession
        {
            UserId = user.UserId,
            SessionId = newSessionId,
            LoginTime = DateTime.UtcNow,

            AccessTokenExpiresAt = tokens.AccessToken.ExpiresAt,

            RefreshTokenHash = _tokenService.HashRefreshToken(tokens.RefreshToken.Token),

            RefreshTokenExpiresAt = tokens.RefreshToken.ExpiresAt,

            IsExpired = false,

        };

        await _db.TblUserSessions.AddAsync(newSession);
        await _db.SaveChangesAsync();

        return new Result<LoginResultModel>
        {
            IsSuccess = true,
            Message = "Token refreshed sucessfully.",

            Data = new LoginResultModel
            {
                User = new LoginResponseModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Role = user.Role,
                    MustChangePassword = user.MustChangePassword,
                },
                Tokens = tokens
            }

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
        
       
    }

    
