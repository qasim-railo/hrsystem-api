using HRSystem.API.Models;

namespace HRSystem.API.DTOs
{
    public class IncrementHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime IncrementDate { get; set; }
        public decimal OldSalary { get; set; }
        public decimal NewSalary { get; set; }
        public string Reason { get; set; }
        public string ApprovedBy { get; set; }

        public Employee Employee { get; set; }
    }

}
