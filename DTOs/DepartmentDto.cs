namespace HRSystem.API.Dtos
{
    public class DepartmentDto
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; } // Optional for display
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
    }
}
