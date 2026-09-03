using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedWorkMinutesAndShiftRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakMinutes",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "Shifts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1));

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                table: "Shifts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDays",
                table: "Shifts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Sunday,Monday,Tuesday,Wednesday,Thursday,Friday,Saturday");

            migrationBuilder.AddColumn<int>(
                name: "ExpectedWorkMinutes",
                table: "AttendanceConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 480);

            migrationBuilder.Sql("""
                UPDATE AttendanceConfigurations
                SET ExpectedWorkMinutes = CAST(ROUND(DefaultWorkingHours * 60, 0) AS int)
                WHERE DefaultWorkingHours > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakMinutes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ExpectedWorkMinutes",
                table: "AttendanceConfigurations");
        }
    }
}
