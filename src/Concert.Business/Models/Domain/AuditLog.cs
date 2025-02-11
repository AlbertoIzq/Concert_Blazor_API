using System.ComponentModel.DataAnnotations;

namespace Concert.Business.Models.Domain
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }  // "Added", "Modified", "Deleted", "SoftDeleted", "Restored"
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Changes { get; set; }  // JSON containing entity changes
    }
}