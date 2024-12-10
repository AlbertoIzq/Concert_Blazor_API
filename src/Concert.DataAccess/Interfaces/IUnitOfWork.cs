using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concert.DataAccess.Interfaces
{
    public interface IUnitOfWork
    {
        ISongRequestRepository SongRequests { get; }

        // Global methods
        Task SaveAsync();
    }
}