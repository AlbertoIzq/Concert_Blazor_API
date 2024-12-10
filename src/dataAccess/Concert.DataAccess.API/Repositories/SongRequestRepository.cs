using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.Interfaces;

namespace Concert.DataAccess.API.Repositories
{
    public class SongRequestRepository : Repository<SongRequest>, ISongRequestRepository
    {
        private readonly ConcertDbContext _concertDbContext;

        public SongRequestRepository(ConcertDbContext concertDbContext) : base(concertDbContext)
        {
            _concertDbContext = concertDbContext;
        }
    }
}