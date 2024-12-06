using Concert.Business.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concert.DataAccess.InMemory.Repositories
{
    public interface ISongRequestsRepository
    {
        List<SongRequest> GetAll();
        SongRequest? GetById(int id);
    }
}
