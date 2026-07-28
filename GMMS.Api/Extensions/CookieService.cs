using GMMS.Domain.Features.Auth.Models;
using Microsoft.AspNetCore.Http;
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

        public void SetAuthCookies(HttpResponse response, TokenResultModel tokens)
        {
            
            //Acess Token Cookie
            response.Cookies.Append(AccessTokenCookie, tokens.AccessToken.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = tokens.AccessToken.ExpiresAt
                });

            //Refresh Token Cookie
            response.Cookies.Append(RefreshTokenCookie, tokens.RefreshToken.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = tokens.RefreshToken.ExpiresAt
                });
        }
        public void ClearAuthCoookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenCookie);
            response.Cookies.Delete(RefreshTokenCookie);
        }

    }
}
