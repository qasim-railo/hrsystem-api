namespace HRSystem.API.DTOs
{
    public class EmployeeProfileDto
    {
        public EmployeeDto Employee { get; set; } = null!;
        public EmploymentDetailDto? Employment { get; set; }
        public EmployeeProfileCountsDto Counts { get; set; } = new();
        public List<EmployeeStatusHistoryDto> StatusHistory { get; set; } = new();
        public List<EmployeeEmploymentHistoryDto> EmploymentHistory { get; set; } = new();
        public List<IncrementHistoryDto> SalaryHistory { get; set; } = new();
        public List<EmployeeDocumentDto> Documents { get; set; } = new();
        public List<AttendanceDto> Attendance { get; set; } = new();
        public List<LeaveRequestResponseDto> Leave { get; set; } = new();
        public List<PayrollDto> Payroll { get; set; } = new();
        public List<EmployeeAssetDto> Assets { get; set; } = new();
        public List<FinalSettlementDto> FinalSettlements { get; set; } = new();
        public List<CustomFieldDefinitionDto> CustomFieldDefinitions { get; set; } = new();
        public List<CustomFieldValueDto> CustomFields { get; set; } = new();
    }

    public class EmployeeProfileCountsDto
    {
        public int Documents { get; set; }
        public int Attendance { get; set; }
        public int Leave { get; set; }
        public int Payroll { get; set; }
        public int Assets { get; set; }
        public int StatusHistory { get; set; }
        public int EmploymentHistory { get; set; }
        public int SalaryHistory { get; set; }
        public int FinalSettlements { get; set; }
    }

    public class EmployeeStatusHistoryDto
    {
        public int EmployeeStatusHistoryId { get; set; }
        public int TenantId { get; set; }
        public int EmployeeId { get; set; }
        public HRSystem.API.Models.EmployeeStatus PreviousStatus { get; set; }
        public HRSystem.API.Models.EmployeeStatus NewStatus { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public int? SupportingDocumentId { get; set; }
    }

    public class EmployeeEmploymentHistoryDto
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
    }
}
