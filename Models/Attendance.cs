namespace HRSystem.API.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckIn { get; set; }
        public TimeSpan CheckOut { get; set; }

        public int OT1 { get; set; } // in minutes
        public int OT2 { get; set; } // in minutes

        public Employee Employee { get; set; }
    }
}
