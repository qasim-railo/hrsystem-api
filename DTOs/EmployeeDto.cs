using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs
{
    public class EmployeeDto
    {
        public int CompanyId { get; set; }
        public int EmployeeId { get; set; }
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
        public DateTime PassportExpiry { get; set; }
        public string PassportCountry { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
    }

}
