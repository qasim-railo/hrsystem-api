using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimePolicyAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OvertimePolicyAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    OvertimePolicyId = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimePolicyAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimePolicyAssignments_OvertimePolicies_OvertimePolicyId",
                        column: x => x.OvertimePolicyId,
                        principalTable: "OvertimePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OvertimePolicyAssignments_OvertimePolicyId",
                table: "OvertimePolicyAssignments",
                column: "OvertimePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimePolicyAssignments_TenantId_OvertimePolicyId_Scope_TargetId_EffectiveFrom",
                table: "OvertimePolicyAssignments",
                columns: new[] { "TenantId", "OvertimePolicyId", "Scope", "TargetId", "EffectiveFrom" },
                unique: true,
                filter: "[TargetId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvertimePolicyAssignments");
        }
    }
}
