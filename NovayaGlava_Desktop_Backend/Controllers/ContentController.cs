using ClassLibForNovayaGlava_Desktop;
using ClassLibForNovayaGlava_Desktop.Comments;
using ClassLibForNovayaGlava_Desktop.UserModel;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    [ApiController]
    [Route("/api/content")]
    public class ContentController : Controller
    {
        IMongoClient _mongoClient;
        IMongoDatabase _novayaGlava;

        // Коллекция для хранения постов пользователей
        IMongoCollection<PostModel> _postsCollection;
        IMongoCollection<UserModel> _usersCollection;
        HttpClient _client;

        public ContentController(MongoClient mongoClient)
        {
            _client = HttpClientSingleton.Client;
            _mongoClient = mongoClient;
            _novayaGlava = mongoClient.GetDatabase("NovayaGlava");
            _postsCollection = _novayaGlava.GetCollection<PostModel>("posts");
            _usersCollection = _novayaGlava.GetCollection<UserModel>("users");
        }

        [HttpPost]
        public async Task<ActionResult> AddNewPost([FromBody] string jsonPost)
        {
            if (jsonPost == null)
                return BadRequest("jsonPost paremetr is null or empty");
            else
            {
                PostModel post = JsonConvert.DeserializeObject<PostModel>(jsonPost);
                await _postsCollection.InsertOneAsync(post);
                return Ok();
            }
        }

        // Получить все посты юзера по id
        [HttpGet]
        public async Task<ActionResult> GetPostsByCreatorId([FromQuery] string creatorId)
        {
            if (creatorId is null || creatorId == string.Empty)
                return BadRequest("creatorId is query string is null or empty");

            if(await UserExists(creatorId))
            {
                IAsyncCursor<PostModel> postCursor = await _postsCollection.FindAsync(p => p.UserId == creatorId);
                List<PostModel> posts = await postCursor.ToListAsync();

                UserModel creator = await GetCreatorById(creatorId);

                var pipeline = new BsonDocument[]
                {
                    new BsonDocument("$lookup", new BsonDocument
                    {
                        {"from", "posts" },
                        {"localField", "_id" },
                        {"foreignField", "UserId" },
                        {"as", "Post" }
                    })
                };
                List<UserPostModel> result = await _usersCollection.Aggregate<UserPostModel>(pipeline).ToListAsync();
                

                if (posts.Count == 0 || posts is null)
                    return Ok();
                string jsonPosts = JsonConvert.SerializeObject(posts);
                return Ok(jsonPosts);
            }
            else
            {
                return BadRequest("Пользователя с таким id не в базе данных");
            }
        }

        // Проверка, есть ли пользователь с данным id 
        private async Task<bool> UserExists(string userId)
        {
            IAsyncCursor<UserModel> userCursor = await _usersCollection.FindAsync(u => u._id == userId);
            UserModel user = userCursor.FirstOrDefault();
            if (user is null)
                return false;
            return true;
        }

        // Получить посты для друга
        // Url
        [HttpGet]
        public async Task<ActionResult> GetPostsForFriendByCreatorId([FromQuery] string creatorId, [FromQuery] string friendId)
        {
            if (creatorId is null || creatorId == string.Empty || friendId is null || friendId == string.Empty)
                return BadRequest("creatorId is null or empty || friendId is null or empty");
            
            if(await IsFriend(creatorId, friendId))
            {
                IAsyncCursor<PostModel> postsCursor = await _postsCollection.FindAsync<PostModel>(p => p.UserId == creatorId && p.Audience == (int)CreatorAudience.Friends);
                List<PostModel> postsForFriends = await postsCursor.ToListAsync();
                if(postsForFriends.Count == 0 || postsForFriends is null)
                    return Ok();
                string jsonPosts = JsonConvert.SerializeObject(postsForFriends);
                return Ok(jsonPosts);
            }
            else
            {
                return Ok("Вы не являетесь другом данного юзера");
            }
        }

        // Проверка, является ли юзер другом креатора
        private async Task<bool> IsFriend(string creatorId, string friendId)
        {
            IAsyncCursor<UserModel> userCursor = await _usersCollection.FindAsync(u => u._id == creatorId);
            UserModel creator = userCursor.FirstOrDefault();
            if (creator.Friends.Contains(friendId))
                return true;
            return false;
        }

        // Получить посты для подписчика
        // Url
        [HttpGet]
        public async Task<ActionResult> GetPostsForSubscriber([FromQuery] string creatorId, [FromQuery] string subscriberId)
        {
            if (creatorId is null || creatorId == string.Empty || subscriberId is null || subscriberId == string.Empty)
                return BadRequest("creatorId is null or empty || friendId is null or empty");

            if(await IsSubscriber(creatorId, subscriberId))
            {
                IAsyncCursor<PostModel> postsCursor = await _postsCollection.FindAsync<PostModel>(p => p.UserId == creatorId && p.Audience == (int)CreatorAudience.Subscribers);
                List<PostModel> postsForSubscribers = await postsCursor.ToListAsync();
                if (postsForSubscribers.Count == 0 || postsForSubscribers is null)
                    return Ok();
                string jsonPosts = JsonConvert.SerializeObject(postsForSubscribers);
                return Ok(jsonPosts);
            }
            else
            {
                return Ok("Вы не являетесь подписчиком данного юзера");
            }
            
        }

        // Проверка, является ли юзер подписчиком креатора
        private async Task<bool> IsSubscriber(string creatorId, string subscriberId)
        {
            IAsyncCursor<UserModel> userCursor = await _usersCollection.FindAsync(u => u._id == creatorId);
            UserModel creator = userCursor.FirstOrDefault();
            if (creator.Subscribers.Contains(subscriberId))
                return true;
            return false;
        }

        private async Task<UserModel> GetCreatorById(string creatorId)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u._id, creatorId);
            IAsyncCursor<UserModel> userCursor = await _usersCollection.FindAsync(filter);
            UserModel user = userCursor.FirstOrDefault();
            return user;
        }
    }
}
