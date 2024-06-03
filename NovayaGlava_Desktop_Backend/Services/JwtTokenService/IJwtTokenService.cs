using ClassLibForNovayaGlava_Desktop.UserModel;

namespace NovayaGlava_Desktop_Backend.Services.JwtTokenService
{
    public interface IJwtTokenService
    {
        public string GenerateJwtToken(UserModel user);
        public string GenerateRefreshToken();
    }
}
