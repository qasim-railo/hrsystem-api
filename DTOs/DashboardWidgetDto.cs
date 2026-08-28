namespace HRSystem.API.DTOs;

public class DashboardWidgetDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool Visible { get; set; }
    public int SortOrder { get; set; }
}

public class DashboardWidgetsDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public DashboardWidgetDto[] Widgets { get; set; } = Array.Empty<DashboardWidgetDto>();
}
