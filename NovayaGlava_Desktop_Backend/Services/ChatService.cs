using MongoDB.Driver;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Services
{
    public interface IChatService
    {

    }
    public class ChatService
    {
        private IMongoClient client;
        IMongoCollection<ChatModel> сhats;
        IConfiguration config;
        IDateTimeService dateTimeService;
        public ChatService(IMongoClient client, IConfiguration config, IDateTimeService dateTimeService)
        {
            this.config = config;
            this.client = client;
            var database = this.client.GetDatabase(config["MongoDb:DatabaseName"]);

            сhats = database.GetCollection<ChatModel>("chats");
            this.dateTimeService = dateTimeService;
        }


    }
}
