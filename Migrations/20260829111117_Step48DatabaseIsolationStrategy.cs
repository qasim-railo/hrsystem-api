using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step48DatabaseIsolationStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Teams",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sections",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Positions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EventCode",
                table: "NotificationTemplates",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Channel",
                table: "NotificationTemplates",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Department",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetCode",
                table: "Assets",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.Sql(@"
WITH ranked AS (
    SELECT e.EmployeeId,
           e.TenantId,
           e.EmployeeCode,
           ROW_NUMBER() OVER (PARTITION BY e.TenantId, TRIM(COALESCE(e.EmployeeCode, '')) ORDER BY e.EmployeeId) AS rn
    FROM dbo.Employees e
)
UPDATE e
SET EmployeeCode = CASE
    WHEN TRIM(COALESCE(e.EmployeeCode, '')) = '' THEN CONCAT('EMP-', e.TenantId, '-', e.EmployeeId)
    WHEN r.rn > 1 THEN CONCAT('EMP-', e.TenantId, '-', e.EmployeeId)
    ELSE e.EmployeeCode
END
FROM dbo.Employees e
LEFT JOIN ranked r ON r.EmployeeId = e.EmployeeId
WHERE TRIM(COALESCE(e.EmployeeCode, '')) = '' OR r.rn > 1; ");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TenantId_SectionId_Name",
                table: "Teams",
                columns: new[] { "TenantId", "SectionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_TenantId_DepartmentId_Name",
                table: "Sections",
                columns: new[] { "TenantId", "DepartmentId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_TenantId_TeamId_Name",
                table: "Positions",
                columns: new[] { "TenantId", "TeamId", "Name" },
                unique: true,
                filter: "[TeamId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TenantId_EventCode_Channel",
                table: "NotificationTemplates",
                columns: new[] { "TenantId", "EventCode", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_TenantId_EmployeeCode",
                table: "Employees",
                columns: new[] { "TenantId", "EmployeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_TenantId_CompanyId_Name",
                table: "Department",
                columns: new[] { "TenantId", "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TenantId_Name",
                table: "Companies",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_TenantId_AssetCode",
                table: "Assets",
                columns: new[] { "TenantId", "AssetCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_TenantId_SectionId_Name",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Sections_TenantId_DepartmentId_Name",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Positions_TenantId_TeamId_Name",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_NotificationTemplates_TenantId_EventCode_Channel",
                table: "NotificationTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Employees_TenantId_EmployeeCode",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Department_TenantId_CompanyId_Name",
                table: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Companies_TenantId_Name",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Assets_TenantId_AssetCode",
                table: "Assets");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EventCode",
                table: "NotificationTemplates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Channel",
                table: "NotificationTemplates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Department",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetCode",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
