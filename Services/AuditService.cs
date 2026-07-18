using System.Text.Json;
using System.Threading.Tasks;
using HRSystem.API.Data;
using HRSystem.API.Models;

namespace HRSystem.API.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        public AuditService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string action, string entity, string entityId, string userId, string details = null)
        {
            var log = new AuditLog
            {
                Action = action,
                Entity = entity,
                EntityId = entityId,
                UserId = userId,
                Details = details,
                CreatedAt = System.DateTime.UtcNow
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}