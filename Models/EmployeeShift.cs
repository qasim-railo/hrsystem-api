namespace HRSystem.API.Models
{
    public class EmployeeShift
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int ShiftId { get; set; }
        public Shift Shift { get; set; }

        public DateTime Date { get; set; }
    }
}
