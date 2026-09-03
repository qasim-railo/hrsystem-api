using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs
{
    public class EmployeeDto
    {
        public int CompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int TenantId { get; set; }
        [Required(ErrorMessage = "DepartmentId is required")]
        public int DepartmentId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string HomeCountryAddress { get; set; } = string.Empty;
        public string HomeCountryPhone { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public DateTime? PassportExpiry { get; set; }
        public string PassportCountry { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        public HRSystem.API.Models.EmployeeStatus Status { get; set; } = HRSystem.API.Models.EmployeeStatus.Draft;
        public HRSystem.API.Models.EmployeeRecordStatus RecordStatus { get; set; } = HRSystem.API.Models.EmployeeRecordStatus.Draft;

        public string CompanyName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public List<string> MatchedFields { get; set; } = new();
        public Dictionary<string, string?>? CustomFields { get; set; }
    }

    public class InitialEmployeeDto
    {
        [Required] public int CompanyId { get; set; }
        [Required] public int DepartmentId { get; set; }
        [Required, StringLength(150)] public string FirstName { get; set; } = string.Empty;
        [Required, StringLength(150)] public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = string.Empty;
        [Required, StringLength(40)] public string Phone { get; set; } = string.Empty;
        [Required, StringLength(2, MinimumLength = 2)] public string Nationality { get; set; } = string.Empty;
        [Required] public int EmployeeCategoryId { get; set; }
        [Required] public int DesignationId { get; set; }
        public Dictionary<string, string?>? CustomFields { get; set; }
    }
}
