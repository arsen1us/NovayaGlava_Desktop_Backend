namespace NovayaGlava_Desktop_Backend.Models
{
    public class UpdateConnectionIdModel
    {
        public string ChatId { get; set; }
        public string ConnectionId { get; set; }
        public string UserId { get; set; }

        public UpdateConnectionIdModel(string chatId, string connectionId, string userId)
        {
            ChatId = chatId;
            ConnectionId = connectionId;
            UserId = userId;
        }
    }
}
