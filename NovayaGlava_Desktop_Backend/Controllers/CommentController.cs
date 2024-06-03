using ClassLibForNovayaGlava_Desktop;
using ClassLibForNovayaGlava_Desktop.Comments;
using ClassLibForNovayaGlava_Desktop.UserModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.ComponentModel.Design;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : Controller
    {
        private MongoClient _database;
        IMongoDatabase _novayaGlavaDb;
        IMongoCollection<CommentModel> _commentsCollection;
        IMongoCollection<PostModel> _postsCollection;

        public CommentController(MongoClient database)
        {
            _database = database;
            _novayaGlavaDb = database.GetDatabase("NovayaGlava");
            _commentsCollection = _novayaGlavaDb.GetCollection<CommentModel>("comments");
        }

        // Получить комментарии для поста по id
        // Url - api/comments/getByPostId/{...}
        [HttpGet("/getByPostId/{postId}")]
        public async Task<ActionResult> GetCommentsByPostId([FromQuery] string postId)
        {
            if (postId is null || postId == "")
                return BadRequest("postId is null or empty");

            IAsyncCursor<CommentModel> commentsCursor = await _commentsCollection.FindAsync<CommentModel>(c => c.PostId == postId);
            List<CommentModel> comments = await commentsCursor.ToListAsync();
            if (comments is null || comments.Count == 0)
                return Ok();

            string jsonComments = JsonConvert.SerializeObject(comments);
            return Ok(jsonComments);
        }

        // Добавить новый комментарий
        [HttpPost("/addNewComment")]
        public async Task<ActionResult> AddNewComment([FromBody] string jsonComment)
        {
            if (string.IsNullOrEmpty(jsonComment))
                return BadRequest("json comment is null or empty");

            CommentModel comment = JsonConvert.DeserializeObject<CommentModel>(jsonComment);
            await _commentsCollection.InsertOneAsync(comment);
            return Ok();
        }

        // Удалить комментарий
        [HttpDelete("/deleteComment")]
        public async Task<ActionResult> DeleteComment([FromBody] string commentId)
        {
            if (string.IsNullOrEmpty(commentId))
                return BadRequest("json comment is null or empty");

            var commentFilter = Builders<CommentModel>.Filter.Eq(c => c._id, commentId);
            
            await _commentsCollection.DeleteOneAsync(commentFilter);
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

            await _commentsCollection.UpdateOneAsync(filter, update);
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
            
            if(await CommentExist(imageModel.CommentId))
            {
                FieldDefinition<CommentModel> field = "AttachmentId";
                // Что обновить (добавить ссылку на вложение к уже имеющимся)
                var update = Builders<CommentModel>.Update.Push(field, imageModel.AttachmentId);

                // У чего обновить 
                var filter = Builders<CommentModel>.Filter.Eq(c => c._id, imageModel.CommentId);

                await _commentsCollection.UpdateOneAsync(filter, update);

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

            await _commentsCollection.UpdateOneAsync(filter, delete);
            return Ok();

        }

        // Существует ли коментарий в бд
        private async Task<bool> CommentExist(string commentId)
        {
            var commentFilter = Builders<CommentModel>.Filter.Eq(c => c._id, commentId);
            IAsyncCursor<CommentModel> commentCursor = await _commentsCollection.FindAsync(commentFilter);

            CommentModel comment = commentCursor.ToList().FirstOrDefault();
            if (comment is null)
                return false;
            return true;
        }

    }
}
