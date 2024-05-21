using ClassLibForNovayaGlava_Desktop;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    // Url - api/messages/{действие}/localdb
    // localdb - добавление записей в коллекции локальной базы данных
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : Controller
    {
        IMongoClient _database;
        IMongoDatabase NovayaGlavaDB;
        IMongoCollection<ChatMessageModel> messages;

        public MessagesController(MongoClient database)
        {
            _database = database;
            NovayaGlavaDB = _database.GetDatabase("NovayaGlava");
            messages = NovayaGlavaDB.GetCollection<ChatMessageModel>("chatMessages");
        }

        // Добавить новое сообщение в локальную базу данных
        // Url - api/messages/add/localdb
        [HttpPost("add/localdb")]
        public async Task<ActionResult> AddMessageLocalDb([FromBody] string jsonMessage)
        {
            if (jsonMessage == null)
                return BadRequest("В теле запроса отсутствует jsonMessage");

            ChatMessageModel message = JsonConvert.DeserializeObject<ChatMessageModel>(jsonMessage);

            await messages.InsertOneAsync(message);
            return Ok();
        }

        // Получение всех сообщений по айди чата из локальной базы данных
        // Url - api/messages/get/{chatId}/localdb
        [HttpGet("get/{chatId}/localdb")]
        public async Task<ActionResult> GetMessagesByChatId(string chatId)
        {
            if (string.IsNullOrEmpty(chatId))
                return BadRequest("В строке запроса отсутствует chatId");

            IAsyncCursor<ChatMessageModel> cursor = await messages.FindAsync(m => m.ChatId == chatId);
            List<ChatMessageModel> chatMessages = cursor.ToList();
            string jsonChatMessages = JsonConvert.SerializeObject(chatMessages);
            return Ok(jsonChatMessages);
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveMessageFromChatById()
        {
            //получаю в теле запроса id сообщения и чата, проверяю время отправки сообщения. И 
            // если время не истекло, то удаляю из чата и из бд;

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> ChangeMessageById()
        {
            //получаю в теле запроса айди чата и само сообщение и обновляю его;
            return Ok();
        }

    }
}
