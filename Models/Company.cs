using System;
namespace HRSystem.API.Models
{
    // your class

    public class Company : ITenantOwned
    {
        public int CompanyId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string? LegalName { get; set; }
        public string? TradeName { get; set; }
        public string? CommercialRegistrationNumber { get; set; }
        public string? Industry { get; set; }
        public int? EmployeeCount { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }

        public ICollection<Employee> Employees { get; set; }
        
        public ICollection<Department> Departments { get; set; }
    }
}