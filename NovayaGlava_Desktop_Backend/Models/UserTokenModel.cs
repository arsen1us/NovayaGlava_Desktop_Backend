namespace NovayaGlava_Desktop_Backend.Models
{
    public class UserTokenModel
    {
        public string UserId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;

        public UserTokenModel(string userId, string token, string refreshToken)
        {
            UserId = userId;
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}
