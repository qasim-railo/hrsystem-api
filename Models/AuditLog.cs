using System;

namespace HRSystem.API.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string Action { get; set; }
        public string Entity { get; set; }
        public string EntityId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Details { get; set; }
    }
}