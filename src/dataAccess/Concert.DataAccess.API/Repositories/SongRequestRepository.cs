using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concert.DataAccess.API.Repositories
{
    public class SongRequestRepository : Repository<SongRequest>, ISongRequestRepository
    {
        private readonly ConcertDbContext _concertDbContext;

        public SongRequestRepository(ConcertDbContext concertDbContext) : base(concertDbContext)
        {
            _concertDbContext = concertDbContext;
        }

        public async Task<SongRequest?> UpdateAsync(int id, SongRequest songRequest)
        {
            // Check if it exists
            var existingSongRequest = await _concertDbContext.SongRequests.FirstOrDefaultAsync(x => x.Id == id);

            if (existingSongRequest == null)
            {
                return null;
            }

            // Assign updated values
            existingSongRequest.Artist = songRequest.Artist;
            existingSongRequest.Title = songRequest.Title;
            existingSongRequest.Genre = songRequest.Genre;
            existingSongRequest.Language = songRequest.Language;
            existingSongRequest.UpdatedAt = DateTime.Now;

            return existingSongRequest;
        }
    }
}