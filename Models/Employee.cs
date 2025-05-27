using System;
namespace HRSystem.API.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string MotherName { get; set; }
        public string HomeCountryAddress { get; set; }
        public string HomeCountryPhone { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyPhone { get; set; }
        public string Email { get; set; }
        public string PassportNumber { get; set; }
        public DateTime PassportExpiry { get; set; }
        public string PassportCountry { get; set; }
        public string PhotoPath { get; set; }

        public EmploymentDetail EmploymentDetail { get; set; }
        public ICollection<EmployeeDocument> EmployeeDocuments { get; set; }
        public ICollection<EmployeeAsset> EmployeeAssets { get; set; }

        public ICollection<EmployeeShift> EmployeeShifts { get; set; }

    }
}
