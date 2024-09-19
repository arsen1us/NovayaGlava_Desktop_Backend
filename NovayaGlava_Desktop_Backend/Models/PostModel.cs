namespace NovayaGlava_Desktop_Backend.Models
{
    public class PostModel
    {
        public string _id { get; set; }
        // morePics - так называется на бэкенде js
        public string Images { get; set; }
        public string UserId { get; set; }

        // Для кого предназначен пост:
        // 0 - для всех
        // 1 - для друзей
        // 2 - для подписчиков
        public int Audience { get; set; } = 0;

        // хз что такое 
        public string WallId { get; set; }
        public DateTime PublicationTime { get; set; }
    }

    public enum CreatorAudience
    {
        All = 0,
        Friends = 1,
        Subscribers = 2
    }
}
