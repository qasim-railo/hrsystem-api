namespace HRSystem.API.DTOs;

public class SelfServiceDto
{
    public object? Profile { get; set; }
    public object[] Attendance { get; set; } = Array.Empty<object>();
    public object[] Leave { get; set; } = Array.Empty<object>();
    public object[] Payslips { get; set; } = Array.Empty<object>();
    public object[] Documents { get; set; } = Array.Empty<object>();
    public object[] Loans { get; set; } = Array.Empty<object>();
    public object[] Assets { get; set; } = Array.Empty<object>();
    public object[] Requests { get; set; } = Array.Empty<object>();
    public object[] Notifications { get; set; } = Array.Empty<object>();
}
