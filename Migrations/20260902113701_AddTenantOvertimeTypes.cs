using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantOvertimeTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OvertimeTypeId",
                table: "OvertimePolicies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OvertimeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Eligibility = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculationMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RateMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaximumMinutes = table.Column<int>(type: "int", nullable: false),
                    ApprovalRequired = table.Column<bool>(type: "bit", nullable: false),
                    PayrollComponentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeTypes_PayrollComponents_PayrollComponentId",
                        column: x => x.PayrollComponentId,
                        principalTable: "PayrollComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO OvertimeTypes (TenantId, Code, Name, Eligibility, CalculationMethod, RateMultiplier, MaximumMinutes, ApprovalRequired, PayrollComponentId, IsActive)
                SELECT tenants.TenantId, defaults.Code, defaults.Name, 'All', 'Multiplier', defaults.RateMultiplier, defaults.MaximumMinutes, CAST(0 AS bit), NULL, CAST(1 AS bit)
                FROM Tenants AS tenants
                CROSS JOIN (VALUES
                    ('OT1', 'Regular OT1', CAST(1.25 AS decimal(18,2)), 120),
                    ('OT2', 'Regular OT2', CAST(1.50 AS decimal(18,2)), 0),
                    ('REST_DAY', 'Rest Day', CAST(1.50 AS decimal(18,2)), 480),
                    ('HOLIDAY', 'Public Holiday', CAST(2.00 AS decimal(18,2)), 480),
                    ('SPECIAL_DAY', 'Special Holiday', CAST(2.00 AS decimal(18,2)), 480)
                ) AS defaults(Code, Name, RateMultiplier, MaximumMinutes)
                WHERE NOT EXISTS (
                    SELECT 1 FROM OvertimeTypes AS types
                    WHERE types.TenantId = tenants.TenantId AND types.Code = defaults.Code
                );

                UPDATE policies
                SET OvertimeTypeId = types.Id
                FROM OvertimePolicies AS policies
                INNER JOIN OvertimeTypes AS types
                    ON types.TenantId = policies.TenantId
                    AND types.Code = policies.Classification
                WHERE policies.OvertimeTypeId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimePolicies_OvertimeTypeId",
                table: "OvertimePolicies",
                column: "OvertimeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeTypes_PayrollComponentId",
                table: "OvertimeTypes",
                column: "PayrollComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeTypes_TenantId_Code",
                table: "OvertimeTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OvertimePolicies_OvertimeTypes_OvertimeTypeId",
                table: "OvertimePolicies",
                column: "OvertimeTypeId",
                principalTable: "OvertimeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OvertimePolicies_OvertimeTypes_OvertimeTypeId",
                table: "OvertimePolicies");

            migrationBuilder.DropTable(
                name: "OvertimeTypes");

            migrationBuilder.DropIndex(
                name: "IX_OvertimePolicies_OvertimeTypeId",
                table: "OvertimePolicies");

            migrationBuilder.DropColumn(
                name: "OvertimeTypeId",
                table: "OvertimePolicies");
        }
    }
}
