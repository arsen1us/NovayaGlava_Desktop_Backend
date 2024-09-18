namespace NovayaGlava_Desktop_Backend.Models
{
    public class ChatModel
    {
        public string _id { get; set; }
        public List<string> Members { get; set; } // сюда добавить, что всего 2 челика может быть в чате;
        public int LastMessageAt { get; set; } //Позиция последнего сообщение?
        public int __v { get; set; } //Что это?
    }
}
