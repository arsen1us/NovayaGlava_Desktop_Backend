using ClassLibForNovayaGlava_Desktop.UserModel;
using MongoDB.Driver;

namespace NovayaGlava_Desktop_Backend.Services.UserService
{
    public interface IUserService
    {
        public Task<UserModel> GetByIdAsync(string userId);
        public Task<UserModel> GetUserByIdAndPasswordAsync(AuthUserModel userModel);
        public Task AddAsync(UserModel user);
        public Task UpdateNickNameByIdAsync(string userId, string newNickName);
        public Task UpdatePasswordByIdAsync(string userId, string newPassword);
        public Task RemoveByIdAsync(string userId);
        
    }
}
