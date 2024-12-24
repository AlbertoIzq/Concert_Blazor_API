using Concert.Business.Models;

namespace Concert.DataAccess.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T> CreateAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        /// <summary>
        /// Delete the entity completely from the database
        /// Afterwards, restoring is not possible
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> HardDeleteAsync(int id);
        /// <summary>
        /// Mark the entity as deleted without deleting it from the database
        /// Entity no longer appears in searches, unless you restore it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> SoftDeleteAsync(int id);
        /// <summary>
        /// Used to restore soft-deleted entities
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T?> RestoreAsync(int id);
    }
}