namespace HRSystem.API.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public int? ApprovedBy { get; set; }
        public string LeaveType { get; set; } // e.g., Annual, Sick, Emergency

        public Employee Employee { get; set; }
    }
}
