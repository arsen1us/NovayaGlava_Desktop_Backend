using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using NovayaGlava_Desktop_Backend.Models;
using NovayaGlava_Desktop_Backend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NovayaGlava_Desktop_Backend.Controllers
{
    // Url - api/users/{действие}/localdb
    // localdb - добавление записей в коллекции локальной базы данных

    [ApiController]
    [Route("/api/users")]
    public class UsersController : ControllerBase
    {
        IConfiguration config;
        private MongoClient client;
        IDistributedCache cache;
        IUserService userService;
        ITokenService tokenService;

        public UsersController(
            IConfiguration config,
            MongoClient database,
            IDistributedCache cache,
            IUserService userService,
            TokenService tokenService)
        {
            this.config = config;
            client = database;
            this.cache = cache;
            this.tokenService = tokenService;
            this.userService = userService;
        }
        // Регистрация пользователя
        // POST: api/users/reg

        [HttpPost("reg")]
        public async Task<IActionResult> Register([FromBody] UserModel user)
        {
            if (user is null || user.NickName is null || user.Password is null || user.Email is null)
                return BadRequest("Не удалось зарегистрировать пользователя. userModel is null или userModel.NickName is null или userModel.Password is null или userModel.Email is null");
            try
            {
                await userService.InsertOneAsync(user);

                string token = tokenService.GenerateJwtToken(user);
                Response.Headers["Authorization"] = token;

                string refreshToken = tokenService.GenerateRefreshToken();
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1)
                };
                Response.Cookies.Append("refresh", refreshToken, cookieOptions);

                return Ok();
            }
            catch (Exception ex)
            {
                // logging
                throw new Exception($"{ex.Message}");
            }
        }
        // Аутентификация пользователя
        // POST - api/users/auth

        [HttpPost("auth")]
        public async Task<IActionResult> Authenticate([FromBody] UserModelAuth userModel)
        {
            if (userModel == null || userModel.Login is null || userModel.Password is null)
                return BadRequest("Не удалось выполнить запрос на аутентификацию. userModel is null или userModel.Login is null или userModel.Password is null");

            try
            {
                UserModel user = await userService.FindAsync(userModel);
                if (user == null)
                    return BadRequest("Такого пользователя нет в базе данных, либо вы ввели неверный логин или пароль");

                string token = tokenService.GenerateJwtToken(user);
                Response.Headers["Authorization"] = token;

                string refreshToken = tokenService.GenerateRefreshToken();
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1)
                };
                Response.Cookies.Append("refresh", refreshToken, cookieOptions);

                return Ok();
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        // Получить всех юзеров из локальной бд
        // GET - api/users

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            try
            {
                List<UserModel> users = await userService.FindAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Получить пользователя по id
        // GET: api/users/{id}

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("userId переданный в теле запроса равен null");
            try
            {
                UserModel user = await userService.FindAsync(id);
                if (user is null)
                    return BadRequest("пользователя с данным id нет в базу данных");

                return Ok(user);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Получить список друзей
        // GET - api/users/friends/{id}

        [HttpGet("friends/{id}")]
        public async Task<ActionResult> GetFriendsAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Parametr userId is null or Empty");
            try
            {
                UserModel user = await userService.FindAsync(id);
                if (user != null)
                {
                    using IAsyncCursor<UserModel> friendsListCursor = await userService.FindAsync(u => u.Friends.Contains(user._id));
                    string jsonFriendsList = JsonConvert.SerializeObject(friendsListCursor);
                    return Ok(jsonFriendsList);
                }
                else
                    return NotFound($"Пользователя с id - [{id}] не было найдено в локальной бд");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
        // Поиск пользователей
        // Url - api/users/input/{input}

        [HttpGet("input/{input}")]
        public async Task<IActionResult> SearchAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
                return BadRequest("Строка ввода для поиска пользователей is null or empty");
            using IAsyncCursor<UserModel> usersCursor = await userService.FindAsync(u => u.NickName.Contains(input));
            List<UserModel> users = usersCursor.ToList();

            string jsonUsers = JsonConvert.SerializeObject(users);
            return Ok(jsonUsers);
        }

        // Это пока не работает

        private async Task<UserModel> GetUserFromCache(string userId)
        {
            IAsyncCursor<UserModel> userCursor;
            string jsonUser = await cache.GetStringAsync(userId);

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
            await cache.SetStringAsync(user._id, jsonString, new DistributedCacheEntryOptions
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
