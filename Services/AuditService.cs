using System.Text.Json;
using System.Threading.Tasks;
using HRSystem.API.Data;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using System.Text.Json.Nodes;

namespace HRSystem.API.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentTenant _tenant;
        public AuditService(AppDbContext context, ICurrentTenant tenant)
        {
            _context = context;
            _tenant = tenant;
        }

        public async Task LogAsync(string action, string entity, string entityId, string userId, string details = null)
        {
            if (_tenant.TenantId is not int tenantId)
                throw new InvalidOperationException("A current tenant is required to write an audit log.");
            var log = new AuditLog
            {
                TenantId = tenantId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                UserId = userId,
                Details = Sanitize(details),
                CreatedAt = System.DateTime.UtcNow
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        private static string Sanitize(string? details)
        {
            if (string.IsNullOrWhiteSpace(details)) return string.Empty;
            try
            {
                var node = JsonNode.Parse(details);
                if (node is not null) Redact(node);
                return node?.ToJsonString() ?? string.Empty;
            }
            catch (JsonException) { }
            return details.Length > 2000 ? details[..2000] : details;
        }

        private static void Redact(JsonNode node)
        {
            if (node is JsonObject obj)
            {
                foreach (var property in obj.ToList())
                {
                    if (IsSensitive(property.Key)) obj[property.Key] = "[REDACTED]";
                    else if (property.Value is not null) Redact(property.Value);
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array)
                    if (item is not null) Redact(item);
            }
        }

        private static bool IsSensitive(string key) =>
            key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("bank", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("identity", StringComparison.OrdinalIgnoreCase);
    }
}