using Concert.Business.Models;
using Concert.Business.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Concert.DataAccess.API.Data
{
    public class ConcertAuthDbContext : IdentityDbContext
    {
        public ConcertAuthDbContext(DbContextOptions<ConcertAuthDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
    }
}