using Microsoft.IdentityModel.Tokens;
using NovayaGlava_Desktop_Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NovayaGlava_Desktop_Backend.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(UserModel user);
        string GenerateRefreshToken();
        Task<string> UpdateJwtTokenAsync(
            string token,
            CancellationToken cancellationToken = default);
        ClaimsPrincipal GetPrincipalExpiredToken(
            string expiredToken,
            CancellationToken cancellationToken = default);
    }
    public class TokenService : ITokenService
    {
        IConfiguration _config;
        IUserService _userService;
        ILogger<TokenService> _logger;
        IDateTimeService dateTimeService;

        public TokenService(
            IConfiguration config, 
            IUserService userService, 
            ILogger<TokenService> logger, 
            IDateTimeService dateTimeService)
        {
            _config = config;
            _userService = userService;
            _logger = logger;
            this.dateTimeService = dateTimeService;
        }

        // Генерация jwt токена
        public string GenerateJwtToken(UserModel user)
        {
            string timestamp = _dateTimeService.GetDateTimeNow();
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]);
                List<Claim> claims = new List<Claim>
                {
                    new Claim("Name", user.NickName),
                    new Claim("Id", user._id)
                };
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = _config["JwtSettings:Issuer"],
                    Audience = _config["JwtSettings:Audience"],
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddMinutes(15),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: [{timestamp}] Error during jwt-token generation. Details: {ex.Message}");
                throw new Exception($"Unexpected error. Details: {ex.Message}");
            }
        }

        // Генерация Refresh токена
        // Получается токен вида: RA2Isc+d/w51Y1vttEk2/rx1DUuOi7CLCvHu41rjbpI=
        public string GenerateRefreshToken()
        {
            string timestamp = _dateTimeService.GetDateTimeNow();
            try
            {
                var randomNumber = new byte[32];
                using (var randomNumberGenerator = RandomNumberGenerator.Create())
                {
                    randomNumberGenerator.GetBytes(randomNumber);
                    return Convert.ToBase64String(randomNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: [{timestamp}] Error during refresh-token generation. Details: {ex.Message}");
                throw new Exception($"Unexpected error. Details: {ex.Message}");
            }
        }

        // Обновление jwt токена
        public async Task<string> UpdateJwtTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            string timestamp = _dateTimeService.GetDateTimeNow();
            try
            {
                var principal = GetPrincipalExpiredToken(token);
                if (principal != null)
                {
                    // log ошибка счмтывания jwt-токен или он поддельный 
                    return null;
                }

                string _id = principal.FindFirst("_id").Value;
                string email = principal.FindFirst("Email").Value;

                if (_id is null || email is null)
                {
                    // log ошибка счмтывания jwt-токен или он поддельный 
                    return null;
                }

                var user = await _userService.FindAsync(_id);

                _logger.LogInformation($"INFO: [{timestamp}] Refresh-token successfully updated");
                return GenerateJwtToken(user);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: [{timestamp}] Jwt-token update with unexpected error. Details: {ex.Message}");
                throw new Exception($"Unexpected error. Details: {ex.Message}");
            }
        }

        // Проверить истёкший jwt-token
        public ClaimsPrincipal GetPrincipalExpiredToken(
            string expiredToken,
            CancellationToken cancellationToken = default)
        {
            string timestamp = _dateTimeService.GetDateTimeNow();
            try
            {
                var tokenValidationParameter = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]))
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(expiredToken, tokenValidationParameter, out SecurityToken securityToken);

                var jwtSecureToken = securityToken as JwtSecurityToken;

                if (jwtSecureToken != null || jwtSecureToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                    return principal;

                var claims = new ClaimsIdentity();

                _logger.LogInformation($"INFO: [{timestamp}] Successfully received principal expired token");
                return new ClaimsPrincipal(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: [{timestamp}] Get principal expired token unexpected error. Details: {ex.Message}");
                throw new Exception($"Unexpected error. Details: {ex.Message}");
            }
        }


    }
}
