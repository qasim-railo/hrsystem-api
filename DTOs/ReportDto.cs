namespace HRSystem.API.DTOs
{
    public class ReportDto
    {
    }
    public class SalaryReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }
        public DateTime Month { get; set; }
        public decimal BasicSalary { get; set; }
        public double OT1Hours { get; set; }
        public double OT2Hours { get; set; }
        public decimal NetSalary { get; set; }
    }
    public class LeaveReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int TotalLeaves { get; set; }
        public int ApprovedLeaves { get; set; }
        public int PendingLeaves { get; set; }
    }
    public class AssetReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string AssetName { get; set; }
        public string AssetCode { get; set; }
        public DateTime AssignedDate { get; set; }
        public string Status { get; set; }
    }
    public class LoanBalanceReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingBalance { get; set; }
    }

}
