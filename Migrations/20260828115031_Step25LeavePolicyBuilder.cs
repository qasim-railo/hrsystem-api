using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step25LeavePolicyBuilder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccrualMethod",
                table: "TenantLeaveTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "AllowEncashment",
                table: "TenantLeaveTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CarryForwardLimit",
                table: "TenantLeaveTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentRequired",
                table: "TenantLeaveTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "TenantLeaveTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                table: "TenantLeaveTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCategory",
                table: "TenantLeaveTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MinimumServiceDays",
                table: "TenantLeaveTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TenantLeaveTypes_TenantId_Name_EffectiveFrom",
                table: "TenantLeaveTypes",
                columns: new[] { "TenantId", "Name", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenantLeaveTypes_TenantId_Name_EffectiveFrom",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "AccrualMethod",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "AllowEncashment",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "CarryForwardLimit",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "DocumentRequired",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "EmployeeCategory",
                table: "TenantLeaveTypes");

            migrationBuilder.DropColumn(
                name: "MinimumServiceDays",
                table: "TenantLeaveTypes");
        }
    }
}
