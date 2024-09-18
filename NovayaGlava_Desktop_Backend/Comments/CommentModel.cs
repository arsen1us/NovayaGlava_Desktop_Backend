namespace NovayaGlava_Desktop_Backend.Comments
{
    public class CommentModel
    {
        public string _id { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string PostId { get; set; } = null!;
        public string UserId { get; set; } = null!;

        // Ограничение на кол-во вложений у комментария - 2 шт
        public List<string> AttachmentId { get; set; }

        // Id сообшения, на которое ответили
        public string ReplyCommentId { get; set; }
    }
}
