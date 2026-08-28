using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step38IntegrationFramework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntegrationConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SecretReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationConnections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConnections_TenantId_ProviderKey",
                table: "IntegrationConnections",
                columns: new[] { "TenantId", "ProviderKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationConnections");
        }
    }
}
