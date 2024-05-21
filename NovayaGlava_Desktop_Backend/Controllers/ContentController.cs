using ClassLibForNovayaGlava_Desktop;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    public class ContentController : Controller
    {
        IMongoClient _mongoClient;
        IMongoDatabase _novayaGlava;

        // Коллекция для хранения постов пользователей
        IMongoCollection<PostModel> _postsCollection;
        HttpClient _client;

        public ContentController(MongoClient mongoClient)
        {
            _client = HttpClientSingleton.Client;
            _mongoClient = mongoClient;
            _novayaGlava = mongoClient.GetDatabase("NovayaGlava");
            _postsCollection = _novayaGlava.GetCollection<PostModel>("posts");
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
    }
}
