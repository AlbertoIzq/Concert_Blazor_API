using Concert.DataAccess.API.Data;
using Concert.DataAccess.Interfaces;

namespace Concert.DataAccess.API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private ConcertDbContext _concertDbContext;

        public ISongRequestRepository SongRequests { get; private set; }

        public UnitOfWork(ConcertDbContext concertDbContext)
        {
            _concertDbContext = concertDbContext;
            SongRequests = new SongRequestRepository(_concertDbContext);
        }

        public async Task SaveAsync()
        {
            await _concertDbContext.SaveChangesAsync();
        }
    }
}