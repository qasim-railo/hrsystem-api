namespace HRSystem.API.Models
{
    /// <summary>
    /// Immutable employment assignment snapshot for an employee.
    /// </summary>
    public class EmployeeEmploymentHistory : ITenantOwned
    {
        public int EmployeeEmploymentHistoryId { get; set; }
        public int TenantId { get; set; }
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentId { get; set; }
        public int? BranchId { get; set; }
        public int? SectionId { get; set; }
        public int? TeamId { get; set; }
        public int? PositionId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public string ChangeReason { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
