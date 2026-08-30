namespace HRSystem.API.DTOs;

public class PlanDto
{
    public int PlanId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxEmployees { get; set; }
    public int MaxUsers { get; set; }
    public int MaxBranches { get; set; }
    public long MaxStorageBytes { get; set; }
    public List<string> FeatureCodes { get; set; } = new();
}

public class UpdatePlanDto
{
    public string? Name { get; set; }
    public int MaxEmployees { get; set; }
    public int MaxUsers { get; set; }
    public int MaxBranches { get; set; }
    public long MaxStorageBytes { get; set; }
    public List<string> FeatureCodes { get; set; } = new();
}

public class CreatePlanDto : UpdatePlanDto
{
    public string Code { get; set; } = string.Empty;
}

public class PlanModuleDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
