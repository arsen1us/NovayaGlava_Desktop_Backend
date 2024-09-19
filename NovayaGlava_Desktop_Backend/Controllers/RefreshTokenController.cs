using ClassLibForNovayaGlava_Desktop;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NovayaGlava_Desktop_Backend.Services;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    [ApiController]
    [Route("/api/refreshToken")]
    public class RefreshTokenController : Controller
    {
        private MongoClient _database;
        IMongoDatabase _novayaGlavaDB;
        IMongoCollection<RefreshTokenModel> _refreshTokensCollection;

        IRefreshTokenService _refreshTokenService;

        public RefreshTokenController(MongoClient database, IRefreshTokenService refreshTokenService)
        {
            _database = database;
            _novayaGlavaDB = database.GetDatabase("NovayaGlava");
            _refreshTokensCollection = _novayaGlavaDB.GetCollection<RefreshTokenModel>("refreshTokens");

            _refreshTokenService = refreshTokenService;
        }

        [HttpGet("/get/{userId}")]
        public async Task<ActionResult> GetByUserId([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("userId is null or empty");

            RefreshTokenModel tokenModel = await _refreshTokenService.GetByUserIdAsync(userId);
            if (tokenModel is null)
                return BadRequest("Не найдено Refresh токена по данномму userId");

            return Ok(tokenModel);
        }

        [HttpPost("/post")]
        public async Task<ActionResult> Add([FromBody] RefreshTokenModel tokenModel)
        {
            if (tokenModel is null || string.IsNullOrEmpty(tokenModel.RefreshToken))
                return BadRequest("Не удалось добавить RefreshTokenModel в базу данных. TokenModel is null или tokenModel.RefreshToken is null or empty");

            await _refreshTokenService.AddAsync(tokenModel);
            return Ok();
        }

        // Обновление Refresh token по userId
        [HttpPut("/update")]
        public async Task<ActionResult> UpdateByUserId([FromBody] RefreshTokenModel tokenModel)
        {
            if (tokenModel is null || string.IsNullOrEmpty(tokenModel.RefreshToken))
                return BadRequest("Не удалось обновить RefreshTokenModel в базе данных. TokenModel is null или tokenModel.RefreshToken is null or empty");

            await _refreshTokenService.Update(tokenModel);
            return Ok();
        }

        [HttpDelete("/delete")]
        public async Task<ActionResult> RemoveByUserId([FromBody] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Не удалось удалить RefreshTokenModel из базы данных. userId is null or empty");

            await _refreshTokenService.RemoveByUserIdAsync(userId);
            return Ok();
        }
    }
}
