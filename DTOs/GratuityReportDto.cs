namespace HRSystem.API.DTOs
{
    public class GratuityReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime EndDate { get; set; }
        public double BasicSalary { get; set; }
        public double GratuityAmount { get; set; }
    }
}
