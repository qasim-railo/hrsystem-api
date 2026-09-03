using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicEmployeeClassifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Positions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCategoryId",
                table: "Positions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeCategoryId",
                table: "EmploymentDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeCategories",
                columns: table => new
                {
                    EmployeeCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCategories", x => x.EmployeeCategoryId);
                });

            migrationBuilder.Sql("""
                INSERT INTO EmployeeCategories (TenantId, Name, Code, Description, IsActive, SortOrder)
                SELECT tenants.TenantId, defaults.Name, defaults.Code, NULL, CAST(1 AS bit), defaults.SortOrder
                FROM Tenants AS tenants
                CROSS JOIN (VALUES
                    ('Labor', 'LABOR', 10),
                    ('Staff', 'STAFF', 20),
                    ('Executive Staff', 'EXECUTIVE_STAFF', 30),
                    ('Managerial', 'MANAGERIAL', 40)
                ) AS defaults(Name, Code, SortOrder)
                WHERE NOT EXISTS (
                    SELECT 1 FROM EmployeeCategories AS categories
                    WHERE categories.TenantId = tenants.TenantId AND categories.Code = defaults.Code
                );

                UPDATE details
                SET EmployeeCategoryId = categories.EmployeeCategoryId
                FROM EmploymentDetails AS details
                INNER JOIN EmployeeCategories AS categories
                    ON categories.TenantId = details.TenantId
                    AND categories.Name = details.Category
                WHERE details.Category IS NOT NULL AND details.Category <> '';

                INSERT INTO Positions (TenantId, Name, Code, Description, IsActive)
                SELECT details.TenantId, details.OfferDesignation,
                    CONCAT('LEGACY-', MIN(details.EmploymentDetailId)), NULL, CAST(1 AS bit)
                FROM EmploymentDetails AS details
                WHERE details.OfferDesignation IS NOT NULL AND details.OfferDesignation <> ''
                    AND NOT EXISTS (
                        SELECT 1 FROM Positions AS positions
                        WHERE positions.TenantId = details.TenantId AND positions.Name = details.OfferDesignation
                    )
                GROUP BY details.TenantId, details.OfferDesignation;

                UPDATE employees
                SET PositionId = positions.PositionId
                FROM Employees AS employees
                INNER JOIN EmploymentDetails AS details ON details.EmployeeId = employees.EmployeeId
                INNER JOIN Positions AS positions
                    ON positions.TenantId = employees.TenantId
                    AND positions.Name = details.OfferDesignation
                WHERE employees.PositionId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_DepartmentId",
                table: "Positions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_EmployeeCategoryId",
                table: "Positions",
                column: "EmployeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentDetails_EmployeeCategoryId",
                table: "EmploymentDetails",
                column: "EmployeeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCategories_TenantId_Code",
                table: "EmployeeCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCategories_TenantId_Name",
                table: "EmployeeCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentDetails_EmployeeCategories_EmployeeCategoryId",
                table: "EmploymentDetails",
                column: "EmployeeCategoryId",
                principalTable: "EmployeeCategories",
                principalColumn: "EmployeeCategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Department_DepartmentId",
                table: "Positions",
                column: "DepartmentId",
                principalTable: "Department",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_EmployeeCategories_EmployeeCategoryId",
                table: "Positions",
                column: "EmployeeCategoryId",
                principalTable: "EmployeeCategories",
                principalColumn: "EmployeeCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentDetails_EmployeeCategories_EmployeeCategoryId",
                table: "EmploymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Department_DepartmentId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_EmployeeCategories_EmployeeCategoryId",
                table: "Positions");

            migrationBuilder.DropTable(
                name: "EmployeeCategories");

            migrationBuilder.DropIndex(
                name: "IX_Positions_DepartmentId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_EmployeeCategoryId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentDetails_EmployeeCategoryId",
                table: "EmploymentDetails");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "EmployeeCategoryId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "EmployeeCategoryId",
                table: "EmploymentDetails");
        }
    }
}
