using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NovayaGlava_Desktop_Backend.Models
{
    public class AuthOptions
    {
        //public string i = "i";
        public const string ISSUER = "default_issuer"; // Издатель токена
        public const string AUDIENCE = "default_audience"; // Потребители токена
        const string KEY = "mysupersecret_secretsecretsecretkey!123"; // Ключ для шифрации
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}
