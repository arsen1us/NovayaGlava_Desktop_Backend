using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using System.Text;
using MailKit;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    [ApiController]
    [Route("/api/mail")]
    public class EmailController : Controller
    {
        IDistributedCache _cache;
        IMongoClient database;
        IMongoDatabase _novayaGlava;
        IMongoCollection<UserModel> _usersCollection;

        public EmailController(IDistributedCache cache, MongoClient database)
        {
            this.database = database;
            _novayaGlava = this.database.GetDatabase("NovayaGlava");
            _usersCollection = _novayaGlava.GetCollection<UserModel>("users");
            _cache = cache;
        }

        [HttpPost("/verificate")]
        public async Task<ActionResult> Verificate([FromBody] string email)
        {
            if (email == null)
                return BadRequest();
            if (!await UserExists(email))
                return Ok("Пользователь с данной почтой ещё не зарегистрирован");
            IAsyncCursor<UserModel> userCursor = await _usersCollection.FindAsync(u => u.Email == email);
            UserModel user = userCursor.FirstOrDefault();

            string verificationCode = GenerateNewVerificationCode();
            return Ok();
        }



        private string GenerateNewVerificationCode()
        {
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            int[] numbers = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            
            for(int i = 0; i < 6; i++)
            {
                sb.Append(rand.Next(0, numbers.Length));
            }
            return sb.ToString();
        }

        private async Task<bool> UserExists(string userEmail)
        {
            IAsyncCursor<UserModel> userCursor = await _usersCollection.FindAsync(u => u.Email == userEmail);
            UserModel user = userCursor.FirstOrDefault();
            if (user is null)
                return false;
            return true;
        }
    }
}
