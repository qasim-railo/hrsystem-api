// Models/Department.cs
using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models
{
	public class Department
	{
		public int DepartmentId { get; set; }

		[Required]
		public string Name { get; set; }

		public string? Description { get; set; }

		// Foreign Key
		public int CompanyId { get; set; }

		// Navigation property
		public Company Company { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
