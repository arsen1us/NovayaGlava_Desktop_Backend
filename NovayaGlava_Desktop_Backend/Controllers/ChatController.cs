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
    [Route("api/chats")]
    public class ChatController : Controller
    {
        private MongoClient _database;
        IMongoDatabase _novayaGlavaDB;
        IMongoCollection<ChatModel> _сhatsCollection;
        IMongoCollection<UserModel> _usersCollection;

        public ChatController(MongoClient database)
        {
            _database = database;
            _novayaGlavaDB = database.GetDatabase("NovayaGlava");
            _сhatsCollection = _novayaGlavaDB.GetCollection<ChatModel>("chats");
            _usersCollection = _novayaGlavaDB.GetCollection<UserModel>("users");
        }

        // Получить все чаты юзера по его id
        // Url - api/chat/getChatsByUserId/localdb?userId={...}
        [Authorize] // Как пример - (Policy = IdentityData.AdminUserPolicyName)
        [HttpGet("get")]
        public async Task<ActionResult> GetChatsById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Не удалось получить список чатов. Параметр userId is null or Empty");

            var chatFilter = Builders<ChatModel>.Filter.AnyEq(chat => chat.Members, userId);
            using IAsyncCursor<ChatModel> chatsCursor = await _сhatsCollection.FindAsync(chatFilter);

            List<ChatModel> chats = chatsCursor.ToList();
            var selectedList = from chat in chats.ToList()
                                    select new ChatUserModel
                                    {
                                        _id = chat._id,
                                        Members = chat.Members,
                                        LastMessageAt = chat.LastMessageAt,
                                        __v = chat.__v,
                                        CompanionId = chat.Members.Where(m => m != userId).First()
                                    };

            return Ok(selectedList.ToList());
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

        
    }
}

