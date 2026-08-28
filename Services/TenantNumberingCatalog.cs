namespace HRSystem.API.Services;

public sealed record NumberingDefinition(string Key, string Label, string DefaultPattern);

public static class TenantNumberingCatalog
{
    public static IReadOnlyList<NumberingDefinition> Definitions { get; } =
    [
        new("employee", "Employee IDs", "EMP-{YEAR}-{NUMBER}"),
        new("leave", "Leave requests", "LV-{NUMBER}"),
        new("loan", "Loans", "LOAN-{NUMBER}"),
        new("payroll", "Payrolls", "PAY-{YEAR}-{MONTH}-{NUMBER}"),
        new("asset", "Assets", "AST-{NUMBER}")
    ];
}
