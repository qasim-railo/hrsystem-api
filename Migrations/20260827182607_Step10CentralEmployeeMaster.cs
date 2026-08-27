using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step10CentralEmployeeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeEmploymentHistories",
                columns: table => new
                {
                    EmployeeEmploymentHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    PositionId = table.Column<int>(type: "int", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeEmploymentHistories", x => x.EmployeeEmploymentHistoryId);
                    table.ForeignKey(
                        name: "FK_EmployeeEmploymentHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEmploymentHistories_EmployeeId",
                table: "EmployeeEmploymentHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeEmploymentHistories_TenantId_EmployeeId_EffectiveFrom",
                table: "EmployeeEmploymentHistories",
                columns: new[] { "TenantId", "EmployeeId", "EffectiveFrom" });

            // Preserve the current employment assignment for employees that already
            // existed before the employment history table was introduced.
            migrationBuilder.Sql(@"
                INSERT INTO EmployeeEmploymentHistories
                    (TenantId, EmployeeId, CompanyId, DepartmentId, BranchId, SectionId, TeamId, PositionId,
                     EffectiveFrom, Category, Designation, ContractType, BasicSalary, GrossSalary,
                     ChangeReason, RecordedAt)
                SELECT e.TenantId, e.EmployeeId, e.CompanyId, e.DepartmentId, e.BranchId, e.SectionId,
                       e.TeamId, e.PositionId, ed.JoiningDate, ISNULL(ed.Category, ''), ISNULL(ed.OfferDesignation, ''),
                       ISNULL(ed.ContractType, ''), ed.BasicSalary, ed.CurrentGrossSalary,
                       'Initial employment record', SYSUTCDATETIME()
                FROM Employees e
                INNER JOIN EmploymentDetails ed ON ed.EmployeeId = e.EmployeeId
                WHERE NOT EXISTS (
                    SELECT 1 FROM EmployeeEmploymentHistories h
                    WHERE h.EmployeeId = e.EmployeeId AND h.TenantId = e.TenantId
                );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeEmploymentHistories");
        }
    }
}
