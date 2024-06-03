using ClassLibForNovayaGlava_Desktop;
using ClassLibForNovayaGlava_Desktop.Comments;
using ClassLibForNovayaGlava_Desktop.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    // Url - api/chat/{действие}/localdb
    // localdb - добавление записей в коллекции локальной базы данных

    [ApiController]
    [Route("api/chat")]
    public class ChatController : Controller
    {
        private MongoClient _database;
        IMongoDatabase _novayaGlavaDB;
        IMongoCollection<ChatModel> _сhatsCollection;
        IMongoCollection<UserModel> _usersCollection;

        public ChatController(MongoClient database)
        {
            this._database = database;
            _novayaGlavaDB = database.GetDatabase("NovayaGlava");
            _сhatsCollection = _novayaGlavaDB.GetCollection<ChatModel>("chats");
            _usersCollection = _novayaGlavaDB.GetCollection<UserModel>("users");
        }

        // Создать чат
        // Добавление в локальную бд
        // Url - api/chat/addChat/localdb
        [HttpPost("addChat/localdb")]
        public async Task<ActionResult> AddNewChatLocalDb([FromBody] string jsonUsersId)
        {
            if (string.IsNullOrEmpty(jsonUsersId))
                return BadRequest("Айди позьзователей отсутствуют в теле запроса, или строка usersId пуста");

            List<string> usersId = JsonConvert.DeserializeObject<List<string>>(jsonUsersId);
            string firstUser = usersId[0];
            string secondUser = usersId[1];

            // Если чат есть
            if (await ChatExists(usersId))
            {
                IAsyncCursor<ChatModel> cursor = await _сhatsCollection.FindAsync<ChatModel>(chat => chat.Members.Contains(firstUser) && chat.Members.Contains(secondUser));
                ChatModel chat = cursor.FirstOrDefault();
                string jsonChat = JsonConvert.SerializeObject(chat);
                return Ok(jsonChat);
            }

            // Если чата нет
            else
            {
                ChatModel chat = new ChatModel
                {
                    _id = Guid.NewGuid().ToString(),
                    Members = usersId,
                    LastMessageAt = 111,
                    __v = 0
                };
                await _сhatsCollection.InsertOneAsync(chat);
                return Ok("Чат успешно создан");
            }
        }

        //Проверка, есть ли чат в локальной бд по id юзеров
        private async Task<bool> ChatExists(List<string> usersId)
        {
            string firstUser = usersId[0];
            string secondUser = usersId[1];
            IAsyncCursor<ChatModel> cursor = await _сhatsCollection.FindAsync<ChatModel>(chat => chat.Members.Contains(firstUser) && chat.Members.Contains(secondUser));
            ChatModel chat = cursor.FirstOrDefault();
            if (chat == null)
                return false;
            return true;
        }

        // Получить все чаты юзера по его id
        // Url - api/chat/getChatsByUserId/localdb?userId={...}
        [Authorize] // Как пример - (Policy = IdentityData.AdminUserPolicyName)
        [HttpGet("getChatsByUserId/localdb")]
        public async Task<ActionResult> GetChatsById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Параметр userId is null or Empty");

            using IAsyncCursor<ChatModel> chatsCursor = await _сhatsCollection.FindAsync(ch => ch.Members.Contains(userId));
            var filter = Builders<UserModel>.Filter.Empty;
            using IAsyncCursor<UserModel> usersCursor = await _usersCollection.FindAsync(filter);

            List<UserModel> users = usersCursor.ToList();
            List<ChatModel> chats = chatsCursor.ToList();

            var joinChatModel = from chat in chats
                                    // Выбираю id собеседника для p2p чата
                                join user in users on chat.Members.Where(id => id != userId).First() equals user._id
                                select new ChatUserModel
                                {
                                    _id = chat._id,
                                    Members = chat.Members,
                                    LastMessageAt = chat.LastMessageAt,
                                    __v = chat.__v,
                                    CompanionId = user._id,
                                    CompanionNickName = user.NickName,
                                };

            List<ChatUserModel> chatUsers = joinChatModel.ToList();
            string jsonChatUsers = JsonConvert.SerializeObject(chatUsers);
            return Ok(jsonChatUsers);
        }
    }
}

