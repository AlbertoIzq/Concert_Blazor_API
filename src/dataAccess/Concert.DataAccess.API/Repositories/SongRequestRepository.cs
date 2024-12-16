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
            if (songRequest.Artist is not null) existingSongRequest.Artist = songRequest.Artist;
            if (songRequest.Title is not null) existingSongRequest.Title = songRequest.Title;
            if (songRequest.Genre is not null) existingSongRequest.Genre = songRequest.Genre;
            if (songRequest.Language is not null) existingSongRequest.Language = songRequest.Language;
            existingSongRequest.UpdatedAt = DateTime.Now;

            return existingSongRequest;
        }
    }
}