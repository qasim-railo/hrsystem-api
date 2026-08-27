using System;

namespace HRSystem.API.Models
{
    public class AuditLog : ITenantOwned
    {
        public int AuditLogId { get; set; }
        public int TenantId { get; set; }
        public string Action { get; set; }
        public string Entity { get; set; }
        public string EntityId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Details { get; set; }
    }
}