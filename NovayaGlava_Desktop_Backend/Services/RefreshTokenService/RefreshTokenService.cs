using ClassLibForNovayaGlava_Desktop;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace NovayaGlava_Desktop_Backend.Services.RefreshTokenService
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private MongoClient _database;
        IMongoDatabase _novayaGlavaDB;
        IMongoCollection<RefreshTokenModel> _refreshTokensCollection;


        public RefreshTokenService(MongoClient database)
        {
            _database = database;
            _novayaGlavaDB = database.GetDatabase("NovayaGlava");
            _refreshTokensCollection = _novayaGlavaDB.GetCollection<RefreshTokenModel>("refreshTokens");
        }

        // Получить Refresh-токен по userId
        public async Task<RefreshTokenModel> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new Exception("userId is null or empty");

            var filter = Builders<RefreshTokenModel>.Filter.Eq(u => u.UserId, userId);
            IAsyncCursor<RefreshTokenModel> tokenCursor = await _refreshTokensCollection.FindAsync<RefreshTokenModel>(filter);

            return tokenCursor.FirstOrDefault();
        }

        // Добавить Refresh-токен
        public async Task AddAsync(RefreshTokenModel refreshTokenModel)
        {
            try 
            { 
                await _refreshTokensCollection.InsertOneAsync(refreshTokenModel);
            }

            catch (Exception ex) 
            {
                throw new Exception($"Не удалось добавить запись в коллекцию refreshTokens. Ошибка: {ex.Message}");
            }
        }

        // Обновить Refresh токен по userId
        public async Task Update(RefreshTokenModel refreshTokenModel)
        {
            var filter = Builders<RefreshTokenModel>.Filter.Eq(r => r.UserId, refreshTokenModel.UserId);
            var update = Builders<RefreshTokenModel>.Update.Set(r => r.RefreshToken, refreshTokenModel.RefreshToken);

            try
            {
                await _refreshTokensCollection.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось обновить запись в коллекции refreshTokens. Ошибка: {ex.Message}");
            }
        }

        // Удалить Refresh токен по userId
        public async Task RemoveByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new Exception("userId is null or empty");

            var filter = Builders<RefreshTokenModel>.Filter.Eq(r => r.UserId, userId);
            try
            {
                await _refreshTokensCollection.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось удалить запись из коллекции refreshTokens. Ошибка: {ex.Message}");
            }
        }

        
    }
}

