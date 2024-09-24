using MongoDB.Driver;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Services
{
    public interface IMessageService : IDatabaseService<MessageModel>
    {

    }
    public class MessageService
    {
        private IMongoClient client;
        IMongoCollection<MessageModel> messages;
        IConfiguration config;
        IDateTimeService dateTimeService;

        public MessageService(IMongoClient client, IConfiguration config, IDateTimeService dateTimeService)
        {
            this.config = config;
            this.client = client;
            var database = this.client.GetDatabase(config["MongoDb:DatabaseName"]);
            messages = database.GetCollection<MessageModel>("comments");
            this.dateTimeService = dateTimeService;
        }
    }
}
