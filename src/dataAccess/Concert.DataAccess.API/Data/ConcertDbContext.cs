using Concert.Business.Models.Domain;
using Microsoft.EntityFrameworkCore;

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
                songRequest.UpdatedAt = new DateTime(2024, 12, 10);
            }

            return _songRequests;
        }
    }
}