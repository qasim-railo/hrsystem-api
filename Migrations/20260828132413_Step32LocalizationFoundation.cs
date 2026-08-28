using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step32LocalizationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "DefaultLanguage",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultLanguage",
                table: "Tenants");
        }
    }
}
