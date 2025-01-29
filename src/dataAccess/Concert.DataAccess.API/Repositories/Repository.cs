using Concert.Business.Models;
using Concert.DataAccess.API.Data;
using Concert.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concert.DataAccess.API.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly ConcertDbContext _concertDbContext;
        private DbSet<T> _dbSet;

        public Repository(ConcertDbContext concertDbContext)
        {
            _concertDbContext = concertDbContext;
            _dbSet = _concertDbContext.Set<T>();
        }

        public async Task<T> CreateAsync(T entity)
        {
            entity.CreatedAt = DateTime.Now;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool includeSoftDeleted)
        {
            return includeSoftDeleted ?
                await _dbSet.IgnoreQueryFilters().ToListAsync() // Ignore the general filter for soft deleted entities
                : await _dbSet.ToListAsync();

        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<T?> HardDeleteAsync(int id)
        {
            // Check if it exists
            var existingEntity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);

            if (existingEntity == null)
            {
                return null;
            }

            // Delete entity
            _dbSet.Remove(existingEntity);

            return existingEntity;
        }

        public async Task<T?> SoftDeleteAsync(int id)
        {
            // Check if it exists
            var existingEntity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);

            if (existingEntity == null)
            {
                return null;
            }

            // Change entity status to deleted
            existingEntity.IsDeleted = true;
            existingEntity.DeletedAt = DateTime.Now;

            return existingEntity;
        }

        public async Task<T?> RestoreAsync(int id)
        {
            // Check if it exists
            var existingEntity = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);

            if (existingEntity == null)
            {
                return null;
            }

            // Restore entity status back to not deleted
            existingEntity.IsDeleted = false;
            existingEntity.DeletedAt = null;

            return existingEntity;
        }
    }
}