namespace HRSystem.API.DTOs
{
    public class AttendanceDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckIn { get; set; }
        public TimeSpan CheckOut { get; set; }
        public int OT1 { get; set; }
        public int OT2 { get; set; }
    }
}
