 

namespace HRSystem.API.DTOs
{
    public class IncrementHistoryDto
    {
        public int EmployeeId { get; set; }
        public DateTime IncrementDate { get; set; }
        public decimal OldSalary { get; set; }
        public decimal NewSalary { get; set; }
        public string Reason { get; set; }
        public string ApprovedBy { get; set; }
        // ✅ Add nested Employee object for frontend
        public SimpleEmployeeDto? Employee { get; set; }
    }

    public class SimpleEmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}