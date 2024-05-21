using ClassLibForNovayaGlava_Desktop;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using NovayaGlava_Desktop_Backend.Models;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    // Url - api/users/{действие}/localdb
    // localdb - добавление записей в коллекции локальной базы данных

    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        IAuthenticationService authenticationService;

        IDistributedCache _cache;

        private MongoClient _database;
        IMongoDatabase _novayaGlavaDB;
        IMongoCollection<UserModel> _usersCollection;

        IDataProtector _dataProtector;

        private const string _tokenSecret = "mysupersecret_secretsecretsecretkey!123";
        private static readonly TimeSpan _tokenLifeTime = TimeSpan.FromMinutes(15);

        public UsersController(MongoClient database,
            IAuthenticationService authenticationService,
            IDistributedCache cashe,
            IDataProtectionProvider dataProtectionProvider)
        {
            this.authenticationService = authenticationService;

            _database = database;
            _novayaGlavaDB = _database.GetDatabase("NovayaGlava");
            _usersCollection = _novayaGlavaDB.GetCollection<UserModel>("users");

            _cache = cashe;

            _dataProtector = dataProtectionProvider.CreateProtector("purprose"); //Успешно создался объект для шифра данные для сеанса
        }

        // Регистрация пользователя в локальной бд
        // Url - api/users/registration/localdb
        [HttpPost("registration/localdb")]
        public async Task<IActionResult> RegistrationLocalDb([FromBody] string jsonUser)
        {
            byte[] key = Encoding.UTF8.GetBytes(_tokenSecret);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtTokenResponseBodyModel responseModel;
            if (string.IsNullOrEmpty(jsonUser))
            {
                responseModel = new JwtTokenResponseBodyModel
                {
                    ok = false,
                    token = string.Empty
                };
                return BadRequest(responseModel);
            }
            UserModel user = JsonConvert.DeserializeObject<UserModel>(jsonUser);
            await _usersCollection.InsertOneAsync(user);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user._id),
                new Claim(ClaimTypes.Name, user.NickName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_tokenLifeTime),
                Issuer = "default_issuer",
                Audience = "default_audience",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            // Добавление хэдера в заголовок
            HttpContext.Response.Headers.Add("Authorization", jwt);
            return Ok(token);
        }

        // Аутентификация пользователя локальная бд
        // Url - api/users/login/localdb
        [HttpPost("login/localdb")]
        public async Task<IActionResult> AuthenticationLocalDb([FromBody] string jsonUser)
        {
            // Получить секретный ключ в виде байтов
            byte[] key = Encoding.UTF8.GetBytes(_tokenSecret);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            JwtTokenResponseBodyModel responseModel;
            if (string.IsNullOrEmpty(jsonUser))
            {
                responseModel = new JwtTokenResponseBodyModel
                {
                    ok = false,
                    token = string.Empty
                };
            }
            UserModelForAuthentication userModel = JsonConvert.DeserializeObject<UserModelForAuthentication>(jsonUser);
            using IAsyncCursor<UserModel> cursor = await _usersCollection.FindAsync(u => u.NickName == userModel.Login && u.Password == userModel.Password);
            UserModel user = cursor.ToList().First();
            if (user == null)
            {
                responseModel = new JwtTokenResponseBodyModel
                {
                    ok = false,
                    token = string.Empty
                };
                string jsonResponseModel = JsonConvert.SerializeObject(responseModel);

                return BadRequest(jsonResponseModel);
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user._id),
                new Claim(ClaimTypes.Name, user.NickName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_tokenLifeTime),
                Issuer = "default_issuer",
                Audience = "default_audience",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            // Добавление хэдера в заголовок
            HttpContext.Response.Headers.Add("Authorization", jwt);

            return Ok(user._id);
        }

        // Метод для авторизации. Если не авторизован - 401, авторизован - 200
        // Url - api/users/authorize
        [Authorize]
        [HttpPost("authorize")]
        public async Task<IActionResult> Authorize()
        {
            return Ok();
        }

        // Получить всех юзеров из локальной бд
        // Url - api/users/allUsers/localdb
        [HttpGet("allUsers/localdb")]
        public async Task<IActionResult> GetUsersListLocalDb()
        {
            using IAsyncCursor<UserModel> cursor = await _usersCollection.FindAsync(Builders<UserModel>.Filter.Empty);
            List<UserModel> users = cursor.ToList();

            string jsonUsers = JsonConvert.SerializeObject(users);
            return Ok(jsonUsers);
        }

        // Получить юзера по айди из локальной бд
        // Url - api/users/userById/localdb?userId={"..."}
        [HttpGet("userById/localdb")]
        public async Task<IActionResult> GetUserByIdLocalDb(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("userId переданный в теле запроса равен null");

            using IAsyncCursor<UserModel> curson = await _usersCollection.FindAsync(u => u._id == userId);
            UserModel user = curson.ToList().First();
            if (user == null)
                return NotFound("юзера с данным id нет в локальной базе данных");
            string jsonUser = JsonConvert.SerializeObject(user);
            return Ok(jsonUser);
        }

        // Получить юзеров по айди из локальной бд
        // Url - api/users/usersById/getFriendsList/localdb?userId={"..."}
        [HttpGet("getFriendsList/localdb")]
        public async Task<ActionResult> GetFriendsListLocalDb(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Parametr userId is null or Empty");

            using IAsyncCursor<UserModel> userCursor = await _usersCollection.FindAsync(u => u._id == userId);
            UserModel user = userCursor.ToList().First();
            if (user != null)
            {
                using IAsyncCursor<UserModel> friendsListCursor = await _usersCollection.FindAsync(u => u.Friends.Contains(user._id));
                string jsonFriendsList = JsonConvert.SerializeObject(friendsListCursor);
                return Ok(jsonFriendsList);
            }
            else
                return NotFound($"Пользователя с id - [{userId}] не было найдено в локальной бд");
        }

        // Поиск пользователей через строку поиска в локальной бд
        // Url - api/users/searchFriends/localdb?input={"..."}
        [HttpGet("searchFriends/localdb")]
        public async Task<IActionResult> SelectUsersBySearchString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return BadRequest("Строка ввода для поиска пользователей is null or empty");
            using IAsyncCursor<UserModel> usersCursor = await _usersCollection.FindAsync(u => u.NickName.Contains(input));
            List<UserModel> users = usersCursor.ToList();

            string jsonUsers = JsonConvert.SerializeObject(users);
            return Ok(jsonUsers);
        }

        //пускай пока так будет
        [HttpGet("getUserById{userId}")]
        public async Task<ActionResult> GetUserById(string userId)
        {
            if (userId == null || userId == string.Empty)
                return BadRequest("userId == null or userId is empty");

            UserModel user = await GetUserFromCache(userId);
            if (user == null)
                return NotFound($"Пользователя с данным id [{userId}] не существует");

            string jsonUser = JsonConvert.SerializeObject(user);
            byte[] buffer = new byte[jsonUser.Length];
            buffer = Encoding.UTF8.GetBytes(jsonUser);

            //HttpContext.Response.Body.Read(buffer, 0, buffer.Length);

            //По факту записывается в тело запроса
            return Ok(jsonUser);
        }



        // Это пока не работает

        private async Task<UserModel> GetUserFromCache(string userId)
        {
            IAsyncCursor<UserModel> userCursor;
            string jsonUser = await _cache.GetStringAsync(userId);

            if (jsonUser == null)
            {
                userCursor = await _usersCollection.FindAsync<UserModel>(u => u._id == userId); // получение из бд
                if (userCursor == null)
                {
                    return null;
                }
                SetUserToCache((UserModel)userCursor.FirstOrDefault()); //добавляю данные о юзере в кэш

                return (UserModel)userCursor.FirstOrDefault(); //возвращаю, полученного из бд юзера
            }
            else
            {
                UserModel user = JsonConvert.DeserializeObject<UserModel>(jsonUser);
                return user;
            }

        }


        //Функция для генерации токена
        private JwtSecurityToken GenerateJwtToken(string userId)
        {
            List<Claim> userClaims = new List<Claim>
            {
                new Claim("userId", userId)
            };
            JwtSecurityToken jwtToken = new JwtSecurityToken
            (
                issuer: "default_issuer",
                audience: "default_audience",
                //claims: userClaims,
                // Время жизни - 2 минуты
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

            return jwtToken;
        }

        private async Task SetUserToCache(UserModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
                //Логирование ошибки
            }

            string jsonString = JsonConvert.SerializeObject(user);
            await _cache.SetStringAsync(user._id, jsonString, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }

        private bool ValidatePassword(string password)
        {
            return true;
        }

        private bool ValidateLogin(string login)
        {
            return true;
        }

        private bool ValidateEmail(string email)
        {
            return true;
        }

        //private async Task<IActionResult> MakeJwtToken()
        //{
        //    using Stream stream = HttpContext.Request.Body;
        //    byte[] buffer = new byte[1024];
        //    string jsonUserData = string.Empty;

        //    await stream.ReadAsync(buffer, 0, buffer.Length);

        //    jsonUserData = Encoding.UTF8.GetString(buffer);

        //    AuthorizationUserModel user = JsonConvert.DeserializeObject<AuthorizationUserModel>(jsonUserData);

        //    if (user == null)
        //        await HttpContext.Response.WriteAsJsonAsync("Ошибка десериализации пользователя");
        //    if (user.Password == null || user.Login == null)
        //        await HttpContext.Response.WriteAsJsonAsync("Bad request: login or password equals null");


        //    List<Claim> claims = new List<Claim>
        //    {
        //        new Claim("Login", user.Login),
        //        new Claim("Password", user.Password)
        //    };
        //    JwtSecurityToken jwtToken = new JwtSecurityToken(
        //        issuer: AuthOptions.ISSUER,
        //        audience: AuthOptions.AUDIENCE,
        //        claims: claims,
        //        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
        //        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

        //    string stringJwtToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        //передать в теле ответа stringJwtToken

        //    return Ok();
        //}


    }
}
