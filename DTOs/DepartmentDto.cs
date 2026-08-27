namespace HRSystem.API.Dtos
{
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; } // Optional for display
        public bool IsActive { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
