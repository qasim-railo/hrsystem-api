using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPeopleOsPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    PlanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxEmployees = table.Column<int>(type: "int", nullable: false),
                    MaxUsers = table.Column<int>(type: "int", nullable: false),
                    MaxBranches = table.Column<int>(type: "int", nullable: false),
                    MaxStorageBytes = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.PlanId);
                });

            migrationBuilder.CreateTable(
                name: "PlanFeatures",
                columns: table => new
                {
                    PlanFeatureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    FeatureCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatures", x => x.PlanFeatureId);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "PlanId", "Code", "MaxBranches", "MaxEmployees", "MaxStorageBytes", "MaxUsers", "Name" },
                values: new object[,]
                {
                    { 1, "ESSENTIAL", 1, 50, 5368709120L, 10, "PeopleOS Essential" },
                    { 2, "PROFESSIONAL", 10, 250, 26843545600L, 50, "PeopleOS Professional" }
                });

            migrationBuilder.InsertData(
                table: "PlanFeatures",
                columns: new[] { "PlanFeatureId", "FeatureCode", "IsEnabled", "PlanId" },
                values: new object[,]
                {
                    { 1, "EMPLOYEE_MANAGEMENT", true, 1 },
                    { 2, "DOCUMENTS", true, 1 },
                    { 3, "LEAVE", true, 1 },
                    { 4, "ATTENDANCE", true, 1 },
                    { 5, "SHIFTS", true, 1 },
                    { 6, "BASIC_PAYROLL", true, 1 },
                    { 7, "PAYSLIPS", true, 1 },
                    { 8, "STANDARD_REPORTS", true, 1 },
                    { 9, "EMPLOYEE_SELF_SERVICE", true, 1 },
                    { 100, "EMPLOYEE_MANAGEMENT", true, 2 },
                    { 101, "DOCUMENTS", true, 2 },
                    { 102, "LEAVE", true, 2 },
                    { 103, "ATTENDANCE", true, 2 },
                    { 104, "SHIFTS", true, 2 },
                    { 105, "BASIC_PAYROLL", true, 2 },
                    { 106, "PAYSLIPS", true, 2 },
                    { 107, "STANDARD_REPORTS", true, 2 },
                    { 108, "EMPLOYEE_SELF_SERVICE", true, 2 },
                    { 109, "LOANS", true, 2 },
                    { 110, "OVERTIME", true, 2 },
                    { 111, "ASSETS", true, 2 },
                    { 112, "GRATUITY", true, 2 },
                    { 113, "FINAL_SETTLEMENT", true, 2 },
                    { 114, "ADVANCED_REPORTS", true, 2 },
                    { 115, "CUSTOM_ROLES", true, 2 },
                    { 116, "WORKFLOWS", true, 2 },
                    { 117, "ORGANIZATION_CHART", true, 2 },
                    { 118, "EXPIRY_ALERTS", true, 2 },
                    { 119, "ADVANCED_AUDIT", true, 2 }
                });

            migrationBuilder.Sql("UPDATE Tenants SET PlanName = 'PeopleOS Essential' WHERE PlanId = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_PlanId",
                table: "Tenants",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_PlanId_FeatureCode",
                table: "PlanFeatures",
                columns: new[] { "PlanId", "FeatureCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Code",
                table: "Plans",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Plans_PlanId",
                table: "Tenants",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Plans_PlanId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "PlanFeatures");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_PlanId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "Tenants");
        }
    }
}
