namespace NovayaGlava_Desktop_Backend.Models
{
    public class AddingFileToServerModel
    {
        public string UserId { get; set; }
        public byte[] Buffer { get; set; }
        public string FilePath { get; set; }
        public string FileExtension { get; set; }

        public string FileName { get; set; }

        public AddingFileToServerModel(string userId, byte[] buffer, string filePath)
        {
            UserId = userId;
            Buffer = buffer;
            FilePath = filePath;
            FileExtension = GetFileExtension();

            FileName = Path.GetFileName(FilePath);
        }

        private string GetFileExtension()
        {
            string[] splitted = FilePath.Split('.');
            return "." + splitted[1];
        }

        private void GetFileName()
        {

        }
    }
}
