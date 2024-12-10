using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concert.DataAccess.API.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ConcertDbContext _concertDbContext;
        private DbSet<T> _dbSet;

        public Repository(ConcertDbContext concertDbContext)
        {
            _concertDbContext = concertDbContext;
            _dbSet = _concertDbContext.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
    }
}