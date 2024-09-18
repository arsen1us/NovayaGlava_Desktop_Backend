namespace NovayaGlava_Desktop_Backend.Comments
{
    public class ImageCommentModel
    {
        public string CommentId { get; set; } = null!;

        // Ограничение на кол-во вложений у комментария - 2 шт
        public List<string> AttachmentId { get; set; } = null!;
    }
}
