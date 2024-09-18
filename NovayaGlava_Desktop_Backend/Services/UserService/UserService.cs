using MongoDB.Driver;

namespace NovayaGlava_Desktop_Backend.Services.UserService
{
    public class UserService : IUserService
    {
        private IMongoClient _database;
        IMongoDatabase _novayaGlavaDB;
        IMongoCollection<UserModel> _usersCollection;

        public UserService(MongoClient database)
        {
            _database = database;
            _novayaGlavaDB = database.GetDatabase("NovayaGlava");
            _usersCollection = _novayaGlavaDB.GetCollection<UserModel>("users");
        }

        public async Task InsertOneAsync(UserModel user)
        {

        }

        public async Task InsertManyAsync(List<UserModel> users)
        {

        }

        public async Task<List<UserModel>> FindAllAsync()
        {
            throw new Exception("Не готово");
        }
        // Получить пользователя по _id
        public async Task<UserModel> FindByIdAsync(string userId)
        {
            try
            {
                var filter = Builders<UserModel>.Filter.Eq(u => u._id, userId);
                IAsyncCursor<UserModel> user = await _usersCollection.FindAsync(filter);

                return user.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Defails: {ex.Message}");
            }
        }

        // Получить пользователя по _id и password
        public async Task<UserModel> FindByIdAndPasswordAsync(UserModelAuth userModel)
        {
            try
            {
                var filterBuilder = Builders<UserModel>.Filter;
                //var filter = filterBuilder.And(filterBuilder.Eq(u => u.NickName, userModel.Login), filterBuilder.Eq(u => u.Password, userModel.Password));
                var filter = Builders<UserModel>.Filter.Eq(u => u.Email, userModel.Login);
                using IAsyncCursor<UserModel> cursor = await _usersCollection.FindAsync(filter);

                return cursor.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception($"Defails: {ex.Message}");
            }
        }

        // Добавить пользователя в базу данных
        public async Task AddAsync(UserModel user)
        {
            if (user is null)
                throw new Exception("Ошибка при добавлении userModel. userModel is null");

        }

        public async Task UpdateAsync(string id, UserModel user)
        {
            throw new Exception("Не готово");
        }

        // Обновить NickName пользователя
        public async Task UpdateNickNameByIdAsync(string userId, string newNickName)
        {
            throw new Exception("Не готово");
        }

        // Обновить пароль пользователя
        public async Task UpdatePasswordByIdAsync(string userId, string newPassword)
        {
            throw new Exception("Не готово");
        }

        // Удалить пользователя по id из базы данных
        public async Task DeleteAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    var filter = Builders<UserModel>.Filter.Eq(u => u._id, id);
                    await _usersCollection.DeleteOneAsync(filter);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Не удалось удалить запись из коллекции users. Ошибка: {ex.Message}");
                }
            }
        }
    }
}
