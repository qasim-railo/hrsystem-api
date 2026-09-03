namespace HRSystem.API.Models;

public class TenantWorkingDay : ITenantOwned
{
    public int TenantWorkingDayId { get; set; }
    public int TenantId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsWorkingDay { get; set; }
    public TimeSpan DefaultStartTime { get; set; }
    public TimeSpan DefaultEndTime { get; set; }
    public int BreakMinutes { get; set; }
}
