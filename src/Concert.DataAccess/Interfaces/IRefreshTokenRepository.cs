using Concert.Business.Models.Domain;

namespace Concert.DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByUserNameAsync(string userName);
        Task<RefreshToken?> UpdateAsync(int id, RefreshToken refreshToken);
        Task<RefreshToken?> DeleteAsync(int id);
    }
}