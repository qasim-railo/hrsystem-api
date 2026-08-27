namespace HRSystem.API.DTOs;

public class RoleAccessDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}
