namespace HRSystem.API.Models
{
    public class Shift : ITenantOwned
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int BreakMinutes { get; set; }
        public string WorkingDays { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
        public DateTime? EffectiveTo { get; set; }
        public string Type { get; set; } // "Staff", "Labor", etc.

        public ICollection<EmployeeShift> EmployeeShifts { get; set; }
    }
}
