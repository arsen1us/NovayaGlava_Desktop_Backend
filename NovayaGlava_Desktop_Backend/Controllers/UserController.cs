using NovayaGlava_Desktop_Backend.Models;
using NovayaGlava_Desktop_Backend.Services.RefreshTokenService;
using NovayaGlava_Desktop_Backend.Services.UserService;
using NovayaGlava_Desktop_Backend.Services.JwtTokenService;

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

namespace NovayaGlava_Desktop_Backend.Controllers
{
    // Url - api/users/{действие}/localdb
    // localdb - добавление записей в коллекции локальной базы данных

    [ApiController]
    [Route("/api")]
    public class UsersController : ControllerBase
    {
        IConfiguration _config;
        private MongoClient _database;
        IMongoCollection<UserModel> _usersCollection;
        IDistributedCache _cache;
        IRefreshTokenService _refreshTokenService;
        IUserService _userService;
        IJwtTokenService _jwtTokenService;
        //Объект для шифра данныx для сеанса
        IDataProtector _dataProtector;

        public UsersController(
            IConfiguration config,
            MongoClient database, 
            IDistributedCache cache, 
            IRefreshTokenService refreshTokenService, 
            IUserService userService, 
            IJwtTokenService jwtTokenService, 
            IDataProtectionProvider dataProtectionProvider)
        {
            _config = config;
            _database = database;
            _usersCollection = _database.GetDatabase(_config["MongoDb:DatabaseName"]).GetCollection<UserModel>("users");
            _cache = cache;
            _refreshTokenService = refreshTokenService;
            _jwtTokenService = jwtTokenService;
            _userService = userService;
            _dataProtector = dataProtectionProvider.CreateProtector("purprose");
        }

        // Регистрация пользователя
        // Url - api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel userModel)
        {
            if (userModel is null || userModel.NickName is null || userModel.Password is null || userModel.Email is null)
                return BadRequest("Не удалось зарегистрировать пользователя. userModel is null или userModel.NickName is null или userModel.Password is null или userModel.Email is null");
            try
            {
                await _userService.InsertOneAsync(userModel);

                string jwtToken = "Bearer " + _jwtTokenService.GenerateJwtToken(userModel);
                string refreshToken = _jwtTokenService.GenerateRefreshToken();
                UserTokenModel token = new UserTokenModel(userModel._id, jwtToken, refreshToken);

                return Ok(token);
            }
            catch (Exception ex)
            {
                // logging
                throw new Exception($"{ex.Message}");
            }
        }

        // Аутентификация пользователя
        // Url - api/users/authenticate
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] UserModelAuth userModel)
        {
            if (userModel == null || userModel.Login is null || userModel.Password is null)
                return BadRequest("Не удалось выполнить запрос на аутентификацию. userModel is null или userModel.Login is null или userModel.Password is null");

            UserModel user = await _userService.GetUserByIdAndPasswordAsync(userModel);
            if (user == null)
                return BadRequest("Такого пользователя нет в базе данных, либо вы ввели неверный логин или пароль");

            string jwtToken = "Bearer" + _jwtTokenService.GenerateJwtToken(user);
            string refreshToken = _jwtTokenService.GenerateRefreshToken();
            UserTokenModel userToken = new UserTokenModel(user._id, jwtToken, refreshToken);

            return Ok(userToken);
        }

        // Метод для авторизации. Если не авторизован - 401, авторизован - 200
        // Url - api/users/authorize
        [Authorize]
        [HttpPost("checkJwt")]
        public async Task<IActionResult> Authenticate()
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

            UserModel user = await _userService.GetByIdAsync(userId);
            if (user is null)
                return BadRequest("пользователя с данным id нет в базу данных");

            return Ok(user);
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
