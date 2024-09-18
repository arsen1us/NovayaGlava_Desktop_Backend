using ClassLibForNovayaGlava_Desktop.UserModel;
using MongoDB.Driver;

namespace NovayaGlava_Desktop_Backend.Services.UserService
{
    public interface IUserService : IDatabaseService<UserModel>
    {
        public Task<UserModel> FindByIdAndPasswordAsync(UserModelAuth userModel);
    }
}
