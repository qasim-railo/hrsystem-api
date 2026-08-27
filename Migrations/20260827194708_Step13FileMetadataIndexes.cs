using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step13FileMetadataIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FileRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "FileRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "FileRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "FileRecords",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_DocumentType",
                table: "FileRecords",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_EntityId",
                table: "FileRecords",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_EntityType",
                table: "FileRecords",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_Status",
                table: "FileRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_TenantId",
                table: "FileRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FileRecords_UploadedAt",
                table: "FileRecords",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileRecords_DocumentType",
                table: "FileRecords");

            migrationBuilder.DropIndex(
                name: "IX_FileRecords_EntityId",
                table: "FileRecords");

            migrationBuilder.DropIndex(
                name: "IX_FileRecords_EntityType",
                table: "FileRecords");

            migrationBuilder.DropIndex(
                name: "IX_FileRecords_Status",
                table: "FileRecords");

            migrationBuilder.DropIndex(
                name: "IX_FileRecords_TenantId",
                table: "FileRecords");

            migrationBuilder.DropIndex(
                name: "IX_FileRecords_UploadedAt",
                table: "FileRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "FileRecords",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
