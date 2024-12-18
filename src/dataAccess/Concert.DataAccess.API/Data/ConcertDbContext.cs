using Concert.Business.Models;
using Concert.Business.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Concert.DataAccess.API.Data
{
    public class ConcertDbContext : DbContext
    {
        public ConcertDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<SongRequest> SongRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed to the database
            modelBuilder.Entity<SongRequest>().HasData(SongRequestsIniData());

            // Apply a filter to excluse soft-deleted entities from queries
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var type = entityType.ClrType;

                if (typeof(BaseEntity).IsAssignableFrom(type))
                {
                    var parameter = Expression.Parameter(type, "e");
                    var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var filterExpression = Expression.Lambda(
                        Expression.Not(isDeletedProperty),
                        parameter
                    );

                    // Done this way because we need to dynamically cast the entity type for the filter to work
                    modelBuilder.Entity(type).HasQueryFilter(filterExpression);
                }
            }
        }

        // Data to seed
        private List<SongRequest> SongRequestsIniData()
        {
            var _songRequests = new List<SongRequest>()
            {
                new SongRequest()
                {
                    Id = 1,
                    Artist = "Ace of base",
                    Title = "All that she wants",
                    Genre = "Reggae",
                    Language = "English",
                },
                new SongRequest()
                {
                    Id = 2,
                    Artist = "And One",
                    Title = "Military fashion show",
                    Genre = "EBM",
                    Language = "English"
                },
                new SongRequest()
                {
                    Id = 3,
                    Artist = "Ascendant Vierge",
                    Title = "Influenceur",
                    Genre = "EDM",
                    Language = "French"
                },
                new SongRequest()
                {
                    Id = 4,
                    Artist = "Boys",
                    Title = "Szalona",
                    Genre = "Disco polo",
                    Language = "Polish"
                },
                new SongRequest()
                {
                    Id = 5,
                    Artist = "Charles Aznavour",
                    Title = "For me Formidable",
                    Genre = "Chanson française",
                    Language = "-"
                }
            };

            foreach (var songRequest in _songRequests)
            {
                songRequest.CreatedAt = new DateTime(2024, 12, 10);
            }

            return _songRequests;
        }
    }
}