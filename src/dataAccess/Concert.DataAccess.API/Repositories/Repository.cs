﻿using Concert.Business.Models;
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

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<T?> DeleteAsync(int id)
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
    }
}