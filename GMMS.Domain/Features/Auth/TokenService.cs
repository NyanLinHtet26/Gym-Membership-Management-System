using GMMS.Database.AppDbContextModels;
using GMMS.Domain.Features.Auth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Domain.Features.Auth
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public TokenResultModel CreateTokens(TblUser user, Guid sessionId)
        {
            var accessToken = GenerateAccessToken(user,sessionId);
            var refreshToken = GenerateRefreshToken();

            return new TokenResultModel
            {
                AccessToken = accessToken,

                RefreshToken = new RefreshTokenModel
                {
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                }
            };
        }
        private AccessTokenModel GenerateAccessToken(TblUser user, Guid sessionId)
        {

            var jwtKey = _configuration["JwtSettings:Key"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];
            var jwtExpiryMinutes = 60;
            int.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out jwtExpiryMinutes);

            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("JWT Key is missing.");
                throw new Exception("JWT key is not configured.");
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

            return new AccessTokenModel
            {
                Token = tokenString,
                ExpiresAt = expiresAt,

            };

        }
        private string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public string HashRefreshToken(string refreshToken)
        {
            return BCrypt.Net.BCrypt.HashPassword(refreshToken);
        }

        public bool VerifyRefreshToken(string refreshToken, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(refreshToken, hash);
        }
    }
}

