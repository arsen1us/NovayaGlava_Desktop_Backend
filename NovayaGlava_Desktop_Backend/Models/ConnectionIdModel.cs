namespace NovayaGlava_Desktop_Backend.Models
{
    public class ConnectionIdModel
    {
        public string _id { get; set; }

        // id собеседника 
        public string UserId { get; set; }

        // id чата
        public string ChatId { get; set; }

        // id подключённого пользователя
        public string ConnectionId { get; set; }

        public ConnectionIdModel(string userId, string chatId, string connectionId)
        {
            _id = Guid.NewGuid().ToString();
            UserId = userId;
            ChatId = chatId;
            ConnectionId = connectionId;
        }
    }
}
