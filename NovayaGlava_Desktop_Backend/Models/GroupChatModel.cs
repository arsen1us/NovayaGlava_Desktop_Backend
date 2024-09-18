namespace NovayaGlava_Desktop_Backend.Models
{
    public class GroupChatModel : ChatModel
    {
        public string Name;
        public string ImageBase64;
        public string[] Muted;

        //Наследуется:
        //id
        //lastMessageAt
        //__v
        //members
    }
}
