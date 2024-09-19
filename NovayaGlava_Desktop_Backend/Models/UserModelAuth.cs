namespace NovayaGlava_Desktop_Backend.Models
{
    public class UserModelAuth
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;

        public UserModelAuth(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}
