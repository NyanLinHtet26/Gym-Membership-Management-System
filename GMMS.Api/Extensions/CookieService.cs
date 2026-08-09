using GMMS.Domain.Features.Auth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMMS.Api.Extensions
{
    public class CookieService
    {
        private const string AccessTokenCookie = "accessToken";
        private const string RefreshTokenCookie = "refreshToken";

        private readonly IConfiguration _configuration;

        public CookieService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SetAuthCookies(HttpResponse response, TokenResultModel tokens)
        {
            var secure = _configuration.GetValue("CookieSettings:Secure", true);

            //Acess Token Cookie
            response.Cookies.Append(AccessTokenCookie, tokens.AccessToken.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = secure,
                    SameSite = SameSiteMode.Lax,
                    Expires = tokens.AccessToken.ExpiresAt
                });

            //Refresh Token Cookie
            response.Cookies.Append(RefreshTokenCookie, tokens.RefreshToken.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = secure,
                    SameSite = SameSiteMode.Lax,
                    Expires = tokens.RefreshToken.ExpiresAt
                });
        }
        public void ClearAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenCookie);
            response.Cookies.Delete(RefreshTokenCookie);
        }

    }
}
