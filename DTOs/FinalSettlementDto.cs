namespace HRSystem.API.DTOs
{
    public class FinalSettlementDto
    {
        public int EmployeeId { get; set; }
        public DateTime SettlementDate { get; set; }
        public double LeaveEncashment { get; set; }
        public double GratuityAmount { get; set; }
        public double Deductions { get; set; }
        public double NetPayable { get; set; }
        public bool EmployeeSigned { get; set; }
        public bool HRSigned { get; set; }
    }

}
