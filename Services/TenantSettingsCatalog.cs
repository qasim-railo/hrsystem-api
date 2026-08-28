namespace HRSystem.API.Services;

public sealed record TenantSettingDefinition(
    string SectionKey,
    string SectionName,
    string Key,
    string Label,
    string ValueType,
    string DefaultValue,
    IReadOnlyList<string> Options);

public static class TenantSettingsCatalog
{
    public static IReadOnlyList<TenantSettingDefinition> Definitions { get; } =
    [
        new("company-profile", "Company Profile", "company.legalName", "Legal name", "text", "", []),
        new("organization", "Organization", "organization.defaultWorkingWeek", "Default working week", "text", "Sunday,Monday,Tuesday,Wednesday,Thursday", []),
        new("users", "Users", "users.allowSelfRegistration", "Allow self-registration", "boolean", "false", []),
        new("roles-permissions", "Roles & Permissions", "security.requirePermissionReview", "Require permission review", "boolean", "true", []),
        new("employee-settings", "Employee Settings", "employees.defaultStatus", "Default employee status", "select", "Active", ["Active", "Probation", "Inactive"]),
        new("payroll", "Payroll", "payroll.currency", "Payroll currency", "text", "QAR", []),
        new("attendance", "Attendance", "attendance.graceMinutes", "Late arrival grace (minutes)", "number", "15", []),
        new("leave", "Leave", "leave.requireApproval", "Require leave approval", "boolean", "true", []),
        new("overtime", "Overtime", "overtime.enabled", "Enable overtime", "boolean", "false", []),
        new("documents", "Document Types", "documents.allowedExtensions", "Allowed extensions", "text", ".pdf,.doc,.docx,.jpg,.jpeg,.png", []),
        new("approvals", "Approval Workflows", "approvals.enabled", "Enable approval workflows", "boolean", "true", []),
        new("notifications", "Notifications", "notifications.emailEnabled", "Enable email notifications", "boolean", "false", []),
        new("numbering", "Numbering", "numbering.employeePattern", "Employee number pattern", "text", "EMP-{YEAR}-{NUMBER}", []),
        new("localization", "Localization", "localization.language", "Default language", "select", "English", ["English", "Arabic"]),
        new("integrations", "Integrations", "integrations.webhooksEnabled", "Enable webhooks", "boolean", "false", []),
        new("subscription", "Subscription", "subscription.usageAlertsEnabled", "Enable usage alerts", "boolean", "true", []),
        new("security", "Security", "security.sessionTimeoutMinutes", "Session timeout (minutes)", "number", "60", [])
    ];

    public static TenantSettingDefinition? Find(string key)
        => Definitions.SingleOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
}
