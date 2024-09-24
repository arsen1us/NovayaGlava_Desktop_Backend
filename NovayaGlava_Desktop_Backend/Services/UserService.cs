using MongoDB.Driver;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Services
{
    public interface IUserService : IDatabaseService<UserModel>
    {
        Task<UserModel> FindAsync(UserModelAuth userModel);
        Task<List<UserModel>> FindFriendsAsync(string id);
        Task InsertFriendAsync(string userId, string id);
        Task DeleteFriendAsync(string userId, string id);
        Task<List<UserModel>> FindSubscribersAsync(string id);
        Task InsertSubscriberAsync(string userId, string id);
        Task DeleteSubscriberAsync(string userId, string id);
    }
    public class UserService : IUserService
    {
        private IMongoClient client;
        IMongoDatabase database;
        IMongoCollection<UserModel> users;
        IConfiguration config;
        IDateTimeService dateTimeService;

        public UserService(MongoClient database, IConfiguration config, DateTimeService dateTimeService)
        {
            client = database;
            this.config = config;
            this.database = database.GetDatabase(config["MongoDb:NovayaGlava"]);
            users = this.database.GetCollection<UserModel>("users");
            this.dateTimeService = dateTimeService;
        }

        public async Task InsertOneAsync(UserModel user)
        {
            try
            {
                if (user is null)
                    throw new ArgumentNullException();

                await users.InsertOneAsync(user);
            }
            catch (Exception ex)
            {
                // logging
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task InsertManyAsync(List<UserModel> usersList)
        {
            try
            {
                if (usersList is null || usersList.Count == 0)
                    throw new ArgumentNullException();
                await users.InsertManyAsync(usersList);
            }
            catch (Exception ex)
            {
                // logging
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task<List<UserModel>> FindAsync()
        {
            try
            {
                var filter = Builders<UserModel>.Filter.Empty;
                var cursor = await users.FindAsync(filter);

                return await cursor.ToListAsync();
            }
            catch (Exception ex)
            {
                // logging
                throw new Exception($"{ex.Message}");
            }
        }
        // Получить пользователя по _id
        public async Task<UserModel> FindAsync(string userId)
        {
            try
            {
                var filter = Builders<UserModel>.Filter.Eq(u => u._id, userId);
                IAsyncCursor<UserModel> user = await users.FindAsync(filter);

                return user.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Defails: {ex.Message}");
            }
        }

        // Получить пользователя по _id и password
        public async Task<UserModel> FindAsync(UserModelAuth userModel)
        {
            try
            {
                var filterBuilder = Builders<UserModel>.Filter;
                //var filter = filterBuilder.And(filterBuilder.Eq(u => u.NickName, userModel.Login), filterBuilder.Eq(u => u.Password, userModel.Password));
                var filter = Builders<UserModel>.Filter.Eq(u => u.Email, userModel.Login);
                using IAsyncCursor<UserModel> cursor = await users.FindAsync(filter);

                return cursor.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Defails: {ex.Message}");
            }
        }

        public async Task UpdateAsync(string id, UserModel user)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || user == null)
                {
                    throw new ArgumentNullException("Переданные параметры равны is null or empty");
                }
                var filter = Builders<UserModel>.Filter.Eq(u => u._id, id);
                var update = Builders<UserModel>.Update
                    .Set(u => u.NickName, user.NickName)
                    .Set(u => u.BirthDate, user.BirthDate)
                    .Set(u => u.Sex, user.Sex)
                    .Set(u => u.PhoneNumber, user.PhoneNumber)
                    .Set(u => u.Age, user.Age)
                    .Set(u => u.AvatarBase64, user.AvatarBase64);

                await users.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось удалить запись из коллекции users. Ошибка: {ex.Message}");
            }
        }

        // Удалить пользователя по id из базы данных
        public async Task DeleteAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    var filter = Builders<UserModel>.Filter.Eq(u => u._id, id);
                    await users.DeleteOneAsync(filter);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Не удалось удалить запись из коллекции users. Ошибка: {ex.Message}");
                }
            }
        }

        public async Task<bool> IsExist(string id)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u._id, id);
            var cursor = await users.FindAsync(filter);
            return cursor.Any() ? true : false;
        }
        //--------------------
        // Методы для работы с друзьями

        public async Task<List<UserModel>> FindFriendsAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || !await IsExist(id))
                {
                    throw new ArgumentNullException("Переданные параметры равны is null or empty");
                }
                var user = await FindAsync(id);
                var filter = Builders<UserModel>.Filter.In(u => u._id, user.Friends);
                var cursor = await users.FindAsync(filter);
                return await cursor.ToListAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task InsertFriendAsync(string userId, string id)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(id) || !await IsExist(userId))
                {
                    throw new ArgumentNullException("Переданные параметры равны is null or empty");
                }
                var filter = Builders<UserModel>.Filter.Eq(u => u._id, userId);
                var update = Builders<UserModel>.Update.AddToSet(u => u.Friends, id);
                await users.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task DeleteFriendAsync(string userId, string id)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(id) || !await IsExist(userId))
                {
                    throw new ArgumentNullException("Переданные параметры равны is null or empty");
                }
                var filter = Builders<UserModel>.Filter.Eq(u => u._id, userId);
                await users.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        //--------------------
        // Методы для работы с подписчиками

        public async Task<List<UserModel>> FindSubscribersAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || !await IsExist(id))
                {
                    throw new ArgumentNullException("Переданные параметры равны is null or empty");
                }
                var user = await FindAsync(id);
                var filter = Builders<UserModel>.Filter.In(u => u._id, user.Subscribers);
                var cursor = await users.FindAsync(filter);
                return await cursor.ToListAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task InsertSubscriberAsync(string userId, string id)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(id) || !await IsExist(userId))
                {
                    throw new ArgumentNullException("Переданные параметры равны is null or empty");
                }
                var filter = Builders<UserModel>.Filter.Eq(u => u._id, userId);
                var update = Builders<UserModel>.Update.AddToSet(u => u.Subscribers, id);
                await users.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        public async Task DeleteSubscriberAsync(string userId, string id)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(id) || !await IsExist(userId))
                {
                    throw new ArgumentNullException("Переданные параметры равны is null or empty");
                }
                var filter = Builders<UserModel>.Filter.Eq(u => u._id, userId);
                await users.DeleteOneAsync(filter);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
