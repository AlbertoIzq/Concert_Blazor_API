using Concert.Business.Models.Domain;

namespace Concert.DataAccess.Interfaces
{
    public interface ISongRequestRepository : IRepository<SongRequest>
    {
        // Add methods that are specific to the SongRequest entity
        // e.g. UpdateAsync() may have extra functionality inside
    }
}