namespace NovayaGlava_Desktop_Backend.Models
{
    public class MessageModel
    {
        public string _id { get; set; }

        // NickName автора сообщения 
        public string Author { get; set; }

        // Список вложений
        public List<AttachmentModel> Attachments { get; set; }
        public string TimeStamp { get; set; }
        public string Content { get; set; }

        // Пересланный комментарий
        public bool ReplyComment { get; set; }
        public string CommentId { get; set; }
        public int __v { get; set; }


        public string ChatId { get; set; } = null!;
    }
}
