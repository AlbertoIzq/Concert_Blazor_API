using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concert.DataAccess.API.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ConcertAuthDbContext _concertAuthDbContext;

        public RefreshTokenRepository(ConcertAuthDbContext concertAuthDbContext)
        {
            _concertAuthDbContext = concertAuthDbContext;
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            await _concertAuthDbContext.RefreshTokens.AddAsync(refreshToken);
            await _concertAuthDbContext.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshToken?> GetByUserNameAsync(string userName)
        {
            return await _concertAuthDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserName == userName);
        }

        public async Task<RefreshToken?> GetByTokenValueAsync(string tokenValue)
        {
            return await _concertAuthDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Value == tokenValue);
        }

        public async Task<RefreshToken?> UpdateAsync(int id, RefreshToken refreshToken)
        {
            // Check if it exists
            var existingRefreshToken = await _concertAuthDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);

            if (existingRefreshToken == null)
            {
                return null;
            }

            // Assign updated values
            existingRefreshToken.UserName = refreshToken.UserName;
            existingRefreshToken.Value = refreshToken.Value;
            existingRefreshToken.ExpiryDate = refreshToken.ExpiryDate;
            await _concertAuthDbContext.SaveChangesAsync();

            return existingRefreshToken;
        }

        public async Task<RefreshToken?> DeleteAsync(int id)
        {
            // Check if it exists
            var existingRefreshToken = await _concertAuthDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);

            if (existingRefreshToken == null)
            {
                return null;
            }

            // Delete entity
            _concertAuthDbContext.RefreshTokens.Remove(existingRefreshToken);
            await _concertAuthDbContext.SaveChangesAsync();

            return existingRefreshToken;
        }
    }
}