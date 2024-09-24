using NovayaGlava_Desktop_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using NovayaGlava_Desktop_Backend.Services;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    [ApiController]
    [Route("/api/token")]
    public class TokenController : Controller
    {
        private string _jwtSecret { get; }
        private string _issuer { get; }
        private string _audience { get; }

        HttpClient client;
        ITokenService tokenService;
        IUserService userService;

        public TokenController(ITokenService tokenService, IUserService userService)
        {
            client = HttpClientSingleton.Client;
            this.tokenService = tokenService;
            this.userService = userService;
        }

        [Authorize]
        [HttpGet("check-authorization")]
        public IActionResult CheckAuthorization()
        {
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenModel tokenModel)
        {
            if(tokenModel == null || string.IsNullOrEmpty(tokenModel.RefreshToken))
                return BadRequest("Parameter tokenModel == null or tokenModel.RefreshToken == null or empty");
            try
            {
                if (Request.Headers.TryGetValue("Authorization", out var values))
                {
                    string oldToken = values[0];
                    string token = await tokenService.UpdateJwtTokenAsync(oldToken);
                    Response.Headers["Authorization"] = token;

                    string refreshToken = tokenService.GenerateRefreshToken();
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(1)
                    };
                    Response.Cookies.Append("refresh", refreshToken, cookieOptions);

                    return Ok();
                }
                return Unauthorized();
            }
            catch(Exception ex)
            {
                // logging
                throw new Exception($"{ex.Message}");
            }
        }

        private ClaimsPrincipal GetPrincipalExpiredToken(string expiredToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(expiredToken, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }



    }
}
