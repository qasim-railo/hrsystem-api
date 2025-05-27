namespace HRSystem.API.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Type { get; set; } // "Staff", "Labor", etc.

        public ICollection<EmployeeShift> EmployeeShifts { get; set; }
    }
}
