namespace HRSystem.API.DTOs;

public class EmployeeCategoryDto
{
    public int EmployeeCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class DesignationDto
{
    public int DesignationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public int? EmployeeCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}
