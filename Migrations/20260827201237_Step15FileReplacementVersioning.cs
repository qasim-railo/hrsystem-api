using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step15FileReplacementVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "FileRecords",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(@"
                WITH Ranked AS (
                    SELECT FileId,
                           ROW_NUMBER() OVER (
                               PARTITION BY TenantId, EntityType, EntityId, DocumentType
                               ORDER BY Version DESC, FileId DESC) AS rn
                    FROM FileRecords
                )
                UPDATE f
                SET IsCurrent = CASE WHEN r.rn = 1 THEN 1 ELSE 0 END,
                    Status = CASE WHEN r.rn = 1 THEN f.Status ELSE 'Inactive' END
                FROM FileRecords f
                INNER JOIN Ranked r ON r.FileId = f.FileId;");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_CurrentVersion",
                table: "FileRecords",
                columns: new[] { "TenantId", "EntityType", "EntityId", "DocumentType", "IsCurrent" },
                unique: true,
                filter: "[IsCurrent] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileRecords_CurrentVersion",
                table: "FileRecords");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "FileRecords");
        }
    }
}
