namespace NovayaGlava_Desktop_Backend.Models
{
    public class TokenModel
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;

        public TokenModel(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}
