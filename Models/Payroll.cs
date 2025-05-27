namespace HRSystem.API.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Month { get; set; } // Use first day of month for reference
        public double BasicSalary { get; set; }
        public double OT1Hours { get; set; }
        public double OT2Hours { get; set; }
        public double OTEarnings { get; set; }
        public double Deductions { get; set; }
        public double NetSalary { get; set; }
        public bool IsApproved { get; set; }

        public Employee Employee { get; set; }
    }
}
