using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class BackfillEmployeeClassifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
