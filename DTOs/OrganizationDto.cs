namespace HRSystem.API.DTOs;

public class OrganizationUnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public int? CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public int ChildCount { get; set; }
    public int EmployeeCount { get; set; }
}

public class CreateOrganizationUnitDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public int? CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class UpdateOrganizationUnitDto : CreateOrganizationUnitDto { }

public class OrganizationDeleteCheckDto
{
    public bool CanDelete { get; set; }
    public string? Reason { get; set; }
    public int ChildCount { get; set; }
    public int EmployeeCount { get; set; }
}
