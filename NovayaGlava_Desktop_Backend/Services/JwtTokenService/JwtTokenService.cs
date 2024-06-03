using ClassLibForNovayaGlava_Desktop.UserModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NovayaGlava_Desktop_Backend.Services.JwtTokenService
{
    public class JwtTokenService : IJwtTokenService
    {
        private string _jwtSecret {get;}
        private string _issuer { get; }
        private string _audience { get; }

        public JwtTokenService(IConfiguration configuration)
        {
            _jwtSecret = configuration["JwtSettings:Key"];
            _issuer = configuration["JwtSettings:Issuer"];
            _audience = configuration["JwtSettings:Audience"];
        }

        public string GenerateJwtToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            List<Claim> claims = new List<Claim>
            {
                new Claim("Name", user.NickName),
                new Claim("Id", user._id)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _issuer,
                Audience = _audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Создание Refresh token
        // Получается токен вида: RA2Isc+d/w51Y1vttEk2/rx1DUuOi7CLCvHu41rjbpI=
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
