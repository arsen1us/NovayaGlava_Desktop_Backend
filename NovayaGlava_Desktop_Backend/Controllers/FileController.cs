using ClassLibForNovayaGlava_Desktop;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private string _src = "C:\\Users\\gamer\\source\\repos\\NovayaGlava-Desktop\\ServerForNovayaGlavaDesktopApp\\Files\\";
        IMongoClient _client;
        IMongoDatabase _novayaGlavaDb;

        // Загрузка файла 
        // Url - api/files/loadFile/localdb
        [HttpPost("loadFile/localdb")]
        public async Task<ActionResult> LoadUserFile([FromBody] string jsonModel)
        {
            if (string.IsNullOrEmpty(jsonModel))
                return BadRequest();

            AddingFileToServerModel model = JsonConvert.DeserializeObject<AddingFileToServerModel>(jsonModel);

            MakeDirectory(model.UserId);

            string filePath = _src + "\\" + model.UserId + "\\" + model.FileName;

            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            fileStream.Write(model.Buffer, 0, model.Buffer.Length);

            return Ok();
        }

        // Загрузка файла на локальный сервер
        // Url - api/files/loadFiles/localdb
        [HttpPost("loadFiles/localdb")]
        public async Task<ActionResult> LoadUserFiles([FromBody] string jsonModel)
        {
            if (string.IsNullOrEmpty(jsonModel))
                return BadRequest();

            List<AddingFileToServerModel> model = JsonConvert.DeserializeObject<List<AddingFileToServerModel>>(jsonModel);

            string userId = model[0].UserId;
            MakeDirectory(userId);
            for (int i = 0; i < model.Count; i++)
            {
                string filePath = _src + "\\" + userId + "\\" + model[i].FileName;

                using FileStream fileStream = new FileStream(filePath, FileMode.Create);
                fileStream.Write(model[i].Buffer, 0, model[i].Buffer.Length);
            }

            return Ok();
        }

        // Функция создания директории
        private void MakeDirectory(string userId)
        {
            string dirPath = _src + userId;
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }


        [HttpGet("getFile/{userId}/localdb")]
        public async Task<ActionResult> GetUserFile(string userId)
        {
            return Ok();
        }

        [HttpGet("getFiles/{userId}/localdb")]
        public async Task<ActionResult> GetUserFiles(string userId)
        {
            return Ok();
        }

    }
}
