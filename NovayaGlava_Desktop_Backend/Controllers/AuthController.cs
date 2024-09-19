using ClassLibForNovayaGlava_Desktop;
using ClassLibForNovayaGlava_Desktop.UserModel;
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
    public class AuthController : Controller
    {
        private string _jwtSecret { get; }
        private string _issuer { get; }
        private string _audience { get; }

        HttpClient _client;
        IRefreshTokenService _refreshTokenService;
        IUserService _userService;
        IJwtTokenService _jwtTokenService;

        public AuthController(IRefreshTokenService refreshTokenService, IUserService userService, IJwtTokenService jwtTokenService)
        {
            _client = HttpClientSingleton.Client;
            _refreshTokenService = refreshTokenService;
            _userService = userService;
            _jwtTokenService = jwtTokenService;
        }

        [Authorize]
        [HttpGet("check-authorization")]
        public IActionResult CheckAuthorization()
        {
            return Ok();
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh([FromBody] TokenModel tokenModel)
        {
            if(tokenModel == null || string.IsNullOrEmpty(tokenModel.RefreshToken))
                return BadRequest("Parameter tokenModel == null or tokenModel.RefreshToken == null or empty");

            var principal = GetPrincipalExpiredToken(tokenModel.Token);
            if (principal == null)
                return BadRequest("jwt token is invalid");

            string userId = principal.FindFirst("Id").Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Вот в этой строчке ошибка - string userId = principal.FindFirst(\"Id\").Value;");

            UserModel user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return BadRequest("Пользователя с данным id нет в базе данных");

            string newJwtToken = _jwtTokenService.GenerateJwtToken(user);
            string newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            RefreshTokenModel refreshToken = await _refreshTokenService.GetByUserIdAsync(user._id);
            refreshToken.RefreshToken = newRefreshToken;
            await _refreshTokenService.Update(refreshToken);

            TokenModel newToken = new TokenModel(newJwtToken, newRefreshToken);
            return Ok(newToken);
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
