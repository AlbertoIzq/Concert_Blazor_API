using Concert.Business.Constants;
using Concert.Business.Models.Domain;
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

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed roles to the database
            modelBuilder.Entity<IdentityRole>().HasData(IdentityRolesIniData());

            // Configure the RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id); // Define primary key
                entity.Property(e => e.UserName).IsRequired();
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.ExpiryDate).IsRequired();
            });
        }

        private List<IdentityRole> IdentityRolesIniData()
        {
            var _roles = new List<IdentityRole>()
            {
                new IdentityRole
                {
                    Id = BackConstants.ADMIN_ROLE_ID,
                    Name = BackConstants.ADMIN_ROLE_NAME,
                    NormalizedName = BackConstants.ADMIN_ROLE_NAME.ToUpper(),
                    ConcurrencyStamp = BackConstants.ADMIN_ROLE_CONCURRENCY_STAMP
                },
                new IdentityRole
                {
                    Id = BackConstants.READER_ROLE_ID,
                    Name = BackConstants.READER_ROLE_NAME,
                    NormalizedName = BackConstants.READER_ROLE_NAME.ToUpper(),
                    ConcurrencyStamp = BackConstants.READER_ROLE_CONCURRENCY_STAMP
                },
                new IdentityRole
                {
                    Id = BackConstants.WRITER_ROLE_ID,
                    Name = BackConstants.WRITER_ROLE_NAME,
                    NormalizedName = BackConstants.WRITER_ROLE_NAME.ToUpper(),
                    ConcurrencyStamp = BackConstants.WRITER_ROLE_CONCURRENCY_STAMP
                }
            };

            return _roles;
        }
    }
}