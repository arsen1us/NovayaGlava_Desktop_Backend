using ClassLibForNovayaGlava_Desktop;

namespace NovayaGlava_Desktop_Backend.Services.RefreshTokenService
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenModel> GetByUserIdAsync(string userId);
        Task AddAsync(RefreshTokenModel refreshTokenModel);
        Task Update(RefreshTokenModel refreshTokenModel);
        Task RemoveByUserIdAsync(string userId);
    }
}
