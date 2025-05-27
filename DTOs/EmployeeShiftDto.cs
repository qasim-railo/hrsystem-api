namespace HRSystem.API.DTOs
{
    public class EmployeeShiftDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ShiftId { get; set; }
        public DateTime Date { get; set; }
    }
}
