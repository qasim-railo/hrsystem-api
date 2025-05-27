namespace HRSystem.API.DTOs
{
    //public class PayrollDto
    //{
    //    public int EmployeeId { get; set; }
    //    public DateTime Month { get; set; }
    //    public double BasicSalary { get; set; }
    //    public double OT1Hours { get; set; }
    //    public double OT2Hours { get; set; }
    //    public double OTEarnings { get; set; }
    //    public double Deductions { get; set; }
    //    public double NetSalary { get; set; }
    //    public bool IsApproved { get; set; }

    //}
    public class PayrollDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Month { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal OT1Amount { get; set; }
        public decimal OT2Amount { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public bool IsApproved { get; set; }
    }

    public class PayrollCreateDto
    {
        public int EmployeeId { get; set; }
        public DateTime Month { get; set; }
    }

}
