using MongoDB.Driver;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Services
{
    public interface ICommentService : IDatabaseService<CommentModel>
    {

    }
    public class CommentService : ICommentService
    {
        private IMongoClient client;
        IMongoCollection<CommentModel> comments;
        IConfiguration config;
        IDateTimeService dateTimeService;

        public CommentService(IMongoClient client, IConfiguration config, IDateTimeService dateTimeService)
        {
            this.config = config;
            this.client = client;
            var database = this.client.GetDatabase(config["MongoDb:DatabaseName"]);
            comments = database.GetCollection<CommentModel>("comments");
            this.dateTimeService = dateTimeService;

        }
        // Получить комментарий по id

        public async Task<CommentModel> FindAsync(string id)
        {
            try
            {
                var filter = Builders<CommentModel>.Filter.Eq(c => c._id, id);
                var cursor = await comments.FindAsync(filter);

                return cursor.First();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Получить все комментарии по id поста

        public async Task<List<CommentModel>> FindByPostIdAsync(string postId)
        {
            try
            {
                var filter = Builders<CommentModel>.Filter.Eq(c => c.PostId, postId);
                var cursor = await comments.FindAsync(filter);

                return await cursor.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Получить все комментарии

        public async Task<List<CommentModel>> FindAllAsync()
        {
            try
            {
                var filter = Builders<CommentModel>.Filter.Empty;
                var cursor = await comments.FindAsync(filter);

                return await cursor.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task InsertManyAsync(List<CommentModel> commentsList)
        {
            try
            {
                if(comments is null || commentsList.Count == 0)
                    throw new ArgumentNullException();
                
                await comments.InsertManyAsync(commentsList);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task InsertOneAsync(CommentModel comment)
        {
            try
            {
                if(comment is null)
                    throw new ArgumentNullException();

                await comments.InsertOneAsync(comment);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Обновить комментарий

        public async Task UpdateAsync(string id, CommentModel comment)
        {
            try
            {
                if(string.IsNullOrEmpty(id) || comment is null)
                    throw new ArgumentNullException();

                var filter = Builders<CommentModel>.Filter.Eq(c => c._id, id);
                var update = Builders<CommentModel>.Update
                    .Set(c => c.Text, comment.Text)
                    .Set(c => c.AttachmentId, comment.AttachmentId)
                    .Set(c => c.ReplyCommentId, comment.ReplyCommentId);

                await comments.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Удалить комментарий
        public async Task DeleteAsync(string id)
        {
            try
            {
                await comments.DeleteOneAsync(comment => comment._id == id);
            }
            catch (MongoCursorNotFoundException ex)
            {
                _logger.LogError($"ERROR: [{timestamp}] MongoDb cursor error. Details - {ex.Message}");
                throw new Exception();
            }
            catch (MongoQueryException ex)
            {
                _logger.LogError($"ERROR: [{timestamp}] Error processing request to MondoDb server. Details - {ex.Message}");
                throw new Exception();
            }
            catch (MongoConnectionException ex)
            {
                _logger.LogError($"ERROR: [{timestamp}] Connecting to MondoDb server error. Details - {ex.Message}");
                throw new Exception();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR: [{timestamp}] An internal server error occurred. Details: {ex.Message}");
                throw new Exception();
            }
        }
    }
}
