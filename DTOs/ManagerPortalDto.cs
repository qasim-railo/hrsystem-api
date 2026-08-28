namespace HRSystem.API.DTOs;

public class ManagerPortalDto
{
    public object[] Team { get; set; } = Array.Empty<object>();
    public object[] PendingLeaveRequests { get; set; } = Array.Empty<object>();
    public object[] PendingAttendanceCorrections { get; set; } = Array.Empty<object>();
    public object[] PendingOvertime { get; set; } = Array.Empty<object>();
    public object[] EmployeesOnLeave { get; set; } = Array.Empty<object>();
    public object[] TodaysTeamAttendance { get; set; } = Array.Empty<object>();
    public object[] TeamCalendar { get; set; } = Array.Empty<object>();
    public object[] DocumentExpiryAlerts { get; set; } = Array.Empty<object>();
}
