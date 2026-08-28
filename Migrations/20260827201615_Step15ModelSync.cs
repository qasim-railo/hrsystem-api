using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step15ModelSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_FileRecords_CurrentVersion",
                table: "FileRecords",
                newName: "IX_FileRecords_TenantId_EntityType_EntityId_DocumentType_IsCurrent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_FileRecords_TenantId_EntityType_EntityId_DocumentType_IsCurrent",
                table: "FileRecords",
                newName: "IX_FileRecords_CurrentVersion");
        }
    }
}
