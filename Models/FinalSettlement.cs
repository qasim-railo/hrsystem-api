namespace HRSystem.API.Models
{
    public class FinalSettlement
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime SettlementDate { get; set; }
        public double LeaveEncashment { get; set; }
        public double GratuityAmount { get; set; }
        public double Deductions { get; set; }
        public double NetPayable { get; set; }
        public bool EmployeeSigned { get; set; }
        public bool HRSigned { get; set; }

        public Employee Employee { get; set; }
    }

}
