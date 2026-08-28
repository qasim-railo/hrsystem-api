using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step24OvertimePolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OvertimePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DayType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RateMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    MaximumApprovedMinutes = table.Column<int>(type: "int", nullable: false),
                    ApprovalRequired = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimePolicies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OvertimePolicies_TenantId_Name_EffectiveFrom",
                table: "OvertimePolicies",
                columns: new[] { "TenantId", "Name", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvertimePolicies");
        }
    }
}
