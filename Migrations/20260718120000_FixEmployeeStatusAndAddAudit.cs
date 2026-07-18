using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    public partial class FixEmployeeStatusAndAddAudit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Entity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            // Map existing employees based on EmploymentDetail.IsActive
            // Active = 2, Archived = 10
            migrationBuilder.Sql(@"UPDATE Employees SET Status = 2 WHERE EmployeeId IN (SELECT EmployeeId FROM EmploymentDetails WHERE IsActive = 1)");
            migrationBuilder.Sql(@"UPDATE Employees SET Status = 10 WHERE EmployeeId IN (SELECT EmployeeId FROM EmploymentDetails WHERE IsActive = 0)");

            // Employees without EmploymentDetail remain Draft (0)

            // Create a migration review table for ambiguous records
            migrationBuilder.CreateTable(
                name: "EmployeeMigrationReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CurrentStatus = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeMigrationReview", x => x.Id);
                });

            // Populate migration review with records that may need HR review (example: EmploymentDetail is null)
            migrationBuilder.Sql(@"
                INSERT INTO EmployeeMigrationReview (EmployeeId, EmployeeName, CompanyId, IsActive, CurrentStatus, Reason)
                SELECT e.EmployeeId, (e.FirstName + ' ' + e.LastName), e.CompanyId, ed.IsActive, e.Status, 'No EmploymentDetail found - requires HR review'
                FROM Employees e
                LEFT JOIN EmploymentDetails ed ON ed.EmployeeId = e.EmployeeId
                WHERE ed.EmployeeId IS NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "EmployeeMigrationReview");
        }
    }
}
