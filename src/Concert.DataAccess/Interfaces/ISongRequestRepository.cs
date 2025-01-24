using Concert.Business.Models.Domain;

namespace Concert.DataAccess.Interfaces
{
    public interface ISongRequestRepository : IRepository<SongRequest>
    {
        // Add methods that are specific to the SongRequest entity
        Task<SongRequest?> UpdateAsync(int id, SongRequest songRequest);
    }
}