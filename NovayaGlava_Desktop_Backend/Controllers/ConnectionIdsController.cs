using ClassLibForNovayaGlava_Desktop;
using ClassLibForNovayaGlava_Desktop.Comments;
using ClassLibForNovayaGlava_Desktop.UserModel;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;


namespace NovayaGlava_Desktop_Backend.Controllers
{
    // Контроллер для обработки щапросов связанных с connectionIds юзеров
    [ApiController]
    [Route("api/connectionIds")]
    public class ConnectionIdsController : Controller
    {
        IMongoClient _client;
        IMongoDatabase _novayaGlava;
        IMongoCollection<ConnectionIdModel> _connectionsId;

        public ConnectionIdsController(MongoClient client)
        {
            _client = client;
            _novayaGlava = _client.GetDatabase("NovayaGlava");
            _connectionsId = _novayaGlava.GetCollection<ConnectionIdModel>("connectionIds");
        }

        // Добавление id подключения юзера в локальную бд
        // Url - /api/connectionIds/add/localdb
        [HttpPost("add/localdb")]
        public async Task<ActionResult> PostConnectionIdLocalDb([FromBody] string jsonConnectionIdModel)
        {
            if (string.IsNullOrEmpty(jsonConnectionIdModel))
                return NotFound("Параметр jsonConnectionModel is null or Empty");

            ConnectionIdModel connectionIdModel = JsonConvert.DeserializeObject<ConnectionIdModel>(jsonConnectionIdModel);

            await _connectionsId.InsertOneAsync(connectionIdModel);
            return Ok();
        }

        // Получение id подключения собеседника из локальной бд
        // Url - /api/connectionIds/getCompanionConnectionId/localdb?chatId="{...}"&companionId="{...}"
        // 
        [HttpGet("getConnectionId/localdb")]
        public async Task<ActionResult> GetConnectionIdLocalDb(string chatId, string userId)
        {
            if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(userId))
                return BadRequest("Параметры запроса chatId или companionId равны null или Empty");

            // Тут выдаёт ошибку
            IAsyncCursor<ConnectionIdModel> connectionIdCursor = await _connectionsId.FindAsync(c => c.ChatId == chatId && c.UserId == userId);
            List<ConnectionIdModel> connectionIds = connectionIdCursor.ToList();
            if (connectionIds.Count == 0)
                return Ok("");

            string connectionId = connectionIds[0].ConnectionId;
            return Ok(connectionId);
        }

        // Обновление id подключения в локальной бд
        // Url - /api/connectionIds/update/localdb
        [HttpPatch("update/localdb")]
        public async Task<ActionResult> UpdateConnectionIdLocalDb([FromBody] string jsonUpdateConnectionIdModel)
        {
            UpdateConnectionIdModel updateConnectionIdModel = JsonConvert.DeserializeObject<UpdateConnectionIdModel>(jsonUpdateConnectionIdModel);

            // Определение фильтра для поиска записи, где поле chatId равно переданному в модели
            var filterByChatId = Builders<ConnectionIdModel>.Filter.Eq("ChatId", updateConnectionIdModel.ChatId);
            // Определение фильтра для поиска записи, где поле userId равно переданному в модели
            var filterByUserId = Builders<ConnectionIdModel>.Filter.Eq("UserId", updateConnectionIdModel.UserId);

            // Объединение фильтров
            var filterCollection = Builders<ConnectionIdModel>.Filter.And(filterByChatId, filterByUserId);


            // Определение того, что должно быть обновлено
            var update = Builders<ConnectionIdModel>.Update.Set("ConnectionId", updateConnectionIdModel.ConnectionId);
            try
            {
                await _connectionsId.UpdateOneAsync(filterByChatId, update);
                return Ok();
            }
            catch
            {
                return BadRequest($"Не удалось обновить запись, со след данными - ChatId - {updateConnectionIdModel.ChatId}, UserId - {updateConnectionIdModel.UserId}");
            }
        }
    }
}
