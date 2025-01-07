using Concert.Business.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Concert.DataAccess.API.Data
{
    public class ConcertAuthDbContext : IdentityDbContext
    {
        public ConcertAuthDbContext(DbContextOptions<ConcertAuthDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var roles = new List<IdentityRole>()
            {
                new IdentityRole
                {
                    Id = BackConstants.ADMIN_ROLE_ID,
                    ConcurrencyStamp = BackConstants.ADMIN_ROLE_ID,
                    Name = BackConstants.ADMIN_ROLE_NAME,
                    NormalizedName = BackConstants.ADMIN_ROLE_NAME.ToUpper()
                },
                new IdentityRole
                {
                    Id = BackConstants.READER_ROLE_ID,
                    ConcurrencyStamp = BackConstants.READER_ROLE_ID,
                    Name = BackConstants.READER_ROLE_NAME,
                    NormalizedName = BackConstants.READER_ROLE_NAME.ToUpper()
                },
                new IdentityRole
                {
                    Id = BackConstants.WRITER_ROLE_ID,
                    ConcurrencyStamp = BackConstants.WRITER_ROLE_ID,
                    Name = BackConstants.WRITER_ROLE_NAME,
                    NormalizedName = BackConstants.WRITER_ROLE_NAME.ToUpper()
                }
            };

            modelBuilder.Entity<IdentityRole>().HasData(roles);
        }
    }
}