namespace HRSystem.API.Models
{
    public class GratuityReport
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public double GratuityAmount { get; set; }
        public DateTime GeneratedDate { get; set; }

        public Employee Employee { get; set; }
    }
}
