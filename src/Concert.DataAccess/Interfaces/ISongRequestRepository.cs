using Concert.Business.Models.Domain;

namespace Concert.DataAccess.Interfaces
{
    public interface ISongRequestRepository : IRepository<SongRequest>
    {
        // Add methods that are specific to the SongRequest entity
        public Task<SongRequest?> UpdateAsync(int id, SongRequest songRequest);
    }
}