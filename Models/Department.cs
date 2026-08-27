// Models/Department.cs
using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models
{
	public class Department : ITenantOwned
	{
		public int DepartmentId { get; set; }
		public int TenantId { get; set; }

		[Required]
		public string Name { get; set; }

		public string? Description { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime? EffectiveFrom { get; set; }
		public DateTime? EffectiveTo { get; set; }
		public DateTime? ArchivedAt { get; set; }

		// Foreign Key
		public int CompanyId { get; set; }
		public int? BranchId { get; set; }

		// Navigation property
		public Company Company { get; set; }
		public Branch? Branch { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public ICollection<Section> Sections { get; set; }
    }
}
