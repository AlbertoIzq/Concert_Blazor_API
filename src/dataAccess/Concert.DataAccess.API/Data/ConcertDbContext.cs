using Concert.Business.Constants;
using Concert.Business.Models;
using Concert.Business.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;

namespace Concert.DataAccess.API.Data
{
    public class ConcertDbContext : DbContext
    {
        private DateTime DATETIME_INI_SEED_TABLE = new DateTime(2024, 12, 4);

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConcertDbContext(DbContextOptions<ConcertDbContext> dbContextOptions,
             IHttpContextAccessor httpContextAccessor) : base(dbContextOptions)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<SongRequest> SongRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed to the database
            modelBuilder.Entity<SongRequest>().HasData(SongRequestsIniData());
            modelBuilder.Entity<AuditLog>().HasData(AuditLogsIniData()); 

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

        /// <summary>
        /// Log any changes in the db in the AuditLogs table
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified ||
                    entry.State == EntityState.Deleted)
                {
                    var auditEntry = new AuditLog
                    {
                        EntityType = entry.Entity.GetType().Name,
                        Action = entry.State.ToString(),
                        UserId = GetCurrentUserId(),
                        Timestamp = DateTime.UtcNow
                    };

                    // Handle newly added entities
                    if (entry.State == EntityState.Added)
                    {
                        // Use a temporary placeholder or skip adding "Id" until the entity is saved
                        auditEntry.RecordId = 0;
                    }
                    else
                    {
                        // For modified and deleted entities, get the real "Id"
                        var recordIdProperty = entry.Property("Id");
                        auditEntry.RecordId = recordIdProperty?.CurrentValue as int? ?? 0;
                    }

                    // Capture changes as JSON
                    if (entry.State == EntityState.Modified)
                    {
                        auditEntry.Changes = JsonSerializer.Serialize(entry.OriginalValues.Properties
                            .ToDictionary(p => p.Name, p => new
                            {
                                OldValue = entry.OriginalValues[p],
                                NewValue = entry.CurrentValues[p]
                            }));
                    }

                    auditEntries.Add(auditEntry);
                }
            }

            // Save the original changes first
            var result = await base.SaveChangesAsync(cancellationToken);

            // Save the audit logs
            if (auditEntries.Any())
            {
                AuditLogs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Helper to get the current user ID
        /// </summary>
        /// <returns></returns>
        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Data to seed SongRequests
        /// </summary>
        /// <returns></returns>
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
                songRequest.CreatedAt = DATETIME_INI_SEED_TABLE;
            }

            return _songRequests;
        }

        /// <summary>
        /// Data to seed AuditLogs
        /// </summary>
        /// <returns></returns>
        private List<AuditLog> AuditLogsIniData()
        {
            var _auditLogs = new List<AuditLog>();

            for (int i = 1; i <= 5; i++)
            {
                var auditLog = new AuditLog()
                {
                    Id = i,
                    RecordId = i
                };
                _auditLogs.Add(auditLog);
            }

            foreach (var songRequest in _auditLogs)
            {
                songRequest.EntityType = "SongRequest";
                songRequest.Action = "Added";
                songRequest.UserId = BackConstants.ADMIN_USER_ID;
                songRequest.Timestamp = DATETIME_INI_SEED_TABLE;
            }

            return _auditLogs;
        }
    }
}