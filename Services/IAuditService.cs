using System.Threading.Tasks;

namespace HRSystem.API.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entity, string entityId, string userId, string details = null);
    }
}