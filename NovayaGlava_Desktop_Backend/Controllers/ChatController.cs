using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    // Url - api/chat/{действие}/localdb
    // localdb - добавление записей в коллекции локальной базы данных

    [ApiController]
    [Route("api/chats")]
    public class ChatController : Controller
    {
        private MongoClient client;
        IMongoDatabase database;
        IMongoCollection<ChatModel> сhats;
        IMongoCollection<UserModel> users;
        IMongoCollection<CommentModel> comments;
        IMongoCollection<MessageModel> messages;

        public ChatController(MongoClient _client)
        {
            client = _client;
            database = _client.GetDatabase("NovayaGlava");
            сhats = database.GetCollection<ChatModel>("chats");
            users = database.GetCollection<UserModel>("users");
            comments = database.GetCollection<CommentModel>("comments");
            messages = database.GetCollection<MessageModel>("messages");
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
            using IAsyncCursor<ChatModel> chatsCursor = await сhats.FindAsync(chatFilter);

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
                IAsyncCursor<ChatModel> cursor = await сhats.FindAsync<ChatModel>(chat => chat.Members.Contains(firstUser) && chat.Members.Contains(secondUser));
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
                await сhats.InsertOneAsync(chat);
                return Ok("Чат успешно создан");
            }
        }

        //Проверка, есть ли чат в локальной бд по id юзеров
        private async Task<bool> ChatExists(List<string> usersId)
        {
            string firstUser = usersId[0];
            string secondUser = usersId[1];
            IAsyncCursor<ChatModel> cursor = await сhats.FindAsync<ChatModel>(chat => chat.Members.Contains(firstUser) && chat.Members.Contains(secondUser));
            ChatModel chat = cursor.FirstOrDefault();
            if (chat == null)
                return false;
            return true;
        }

        // Добавить новое сообщение в локальную базу данных
        // Url - api/messages/add/localdb
        [HttpPost("add/localdb")]
        public async Task<ActionResult> AddMessageLocalDb([FromBody] string jsonMessage)
        {
            if (jsonMessage == null)
                return BadRequest("В теле запроса отсутствует jsonMessage");

            MessageModel message = JsonConvert.DeserializeObject<MessageModel>(jsonMessage);

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

            IAsyncCursor<MessageModel> cursor = await messages.FindAsync(m => m.ChatId == chatId);
            List<MessageModel> chatMessages = cursor.ToList();
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

        [HttpGet]
        [Route("/getByPostId/{postId}")]
        public async Task<ActionResult> GetCommentsByPostId([FromQuery] string postId)
        {
            if (postId is null || postId == "")
                return BadRequest("postId is null or empty");

            IAsyncCursor<CommentModel> commentsCursor = await comments.FindAsync<CommentModel>(c => c.PostId == postId);
            List<CommentModel> result = await commentsCursor.ToListAsync();
            if (result is null || result.Count == 0)
                return Ok();

            string jsonComments = JsonConvert.SerializeObject(result);
            return Ok(jsonComments);
        }

        // Добавить новый комментарий
        [HttpPost("/addNewComment")]
        public async Task<ActionResult> AddNewComment([FromBody] string jsonComment)
        {
            if (string.IsNullOrEmpty(jsonComment))
                return BadRequest("json comment is null or empty");

            CommentModel comment = JsonConvert.DeserializeObject<CommentModel>(jsonComment);
            await comments.InsertOneAsync(comment);
            return Ok();
        }

        // Удалить комментарий
        [HttpDelete("/deleteComment")]
        public async Task<ActionResult> DeleteComment([FromBody] string commentId)
        {
            if (string.IsNullOrEmpty(commentId))
                return BadRequest("json comment is null or empty");

            var commentFilter = Builders<CommentModel>.Filter.Eq(c => c._id, commentId);

            await comments.DeleteOneAsync(commentFilter);
            return Ok();
        }

        // Обновить текст в комментарии
        // 
        [HttpPut("/updateText/localdb")]
        public async Task<ActionResult> UpdateCommentText([FromBody] string json)
        {
            if (string.IsNullOrEmpty(json))
                return BadRequest("jsonAddImageToCommentModel is null or empty");
            CommentTextModel model = JsonConvert.DeserializeObject<CommentTextModel>(json);
            if (model == null)
                return BadRequest();
            var filter = Builders<CommentModel>.Filter.Eq(c => c._id, model.CommentId);
            var update = Builders<CommentModel>.Update.Set(c => c.Text, model.Text);

            await comments.UpdateOneAsync(filter, update);
            return Ok();
        }

        // Обновить картинку в комментарии
        // 
        [HttpPut("/updateComment/localdb")]
        public async Task<ActionResult> UpdateCommentImage([FromBody] string json)
        {
            if (string.IsNullOrEmpty(json))
                return BadRequest("List<jsonAddImageToCommentModel> is null or empty");
            List<ImageCommentModel> imagesModel = JsonConvert.DeserializeObject<List<ImageCommentModel>>(json);
            if (imagesModel == null)
                return BadRequest();
            return Ok();
        }

        // Добавление фотографии к комментарию
        // Url - api/comments/addImageToComment/{...}
        [HttpPost("/addImageToComment/{commentId}")]
        public async Task<ActionResult> AddImageToComment([FromBody] string json)
        {
            if (string.IsNullOrEmpty(json))
                return BadRequest("jsonAddImageToCommentModel is null or empty");

            ImageCommentModel imageModel = JsonConvert.DeserializeObject<ImageCommentModel>(json);
            if (imageModel is null || imageModel.CommentId == null || imageModel.AttachmentId == null)
                return BadRequest();

            if (await CommentExist(imageModel.CommentId))
            {
                FieldDefinition<CommentModel> field = "AttachmentId";
                // Что обновить (добавить ссылку на вложение к уже имеющимся)
                var update = Builders<CommentModel>.Update.Push(field, imageModel.AttachmentId);

                // У чего обновить 
                var filter = Builders<CommentModel>.Filter.Eq(c => c._id, imageModel.CommentId);

                await comments.UpdateOneAsync(filter, update);

                return Ok();
            }
            else
            {
                return NotFound("Comment is not exist in database");
            }
        }

        // Удаление фотографии у комментария
        // Url - api/comments/addImageToComment/{...}
        [HttpPost("/deleteImage/localdb")]
        public async Task<ActionResult> DeleteCommentImage([FromBody] string json)
        {
            if (string.IsNullOrEmpty(json))
                return BadRequest("jsonAddImageToCommentModel is null or empty");

            FieldDefinition<CommentModel> field = "AttachmentId";

            ImageCommentModel imageModel = JsonConvert.DeserializeObject<ImageCommentModel>(json);
            var delete = Builders<CommentModel>.Update.PullFilter(c => c.AttachmentId, a => a == imageModel.AttachmentId[0] || a == imageModel.AttachmentId[1]);
            var filter = Builders<CommentModel>.Filter.Eq(comment => comment._id, imageModel.CommentId);

            await comments.UpdateOneAsync(filter, delete);
            return Ok();

        }

        // Существует ли коментарий в бд
        private async Task<bool> CommentExist(string commentId)
        {
            var commentFilter = Builders<CommentModel>.Filter.Eq(c => c._id, commentId);
            IAsyncCursor<CommentModel> commentCursor = await comments.FindAsync(commentFilter);

            CommentModel comment = commentCursor.ToList().FirstOrDefault();
            if (comment is null)
                return false;
            return true;
        }
    }
}

