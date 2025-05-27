using System;
namespace HRSystem.API.Models
{
    // your class

    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public ICollection<Employee> Employees { get; set; }
        
        public ICollection<Department> Departments { get; set; }
    }
}