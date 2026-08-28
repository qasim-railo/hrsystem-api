using HRSystem.API.Models;

namespace HRSystem.API.Models.Auth;

public class AppUser
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? ArchivedAt { get; set; }
    public string Language { get; set; } = "en";
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
