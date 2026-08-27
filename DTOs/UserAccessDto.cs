namespace HRSystem.API.DTOs;

public class UserAccessDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}
