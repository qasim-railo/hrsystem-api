using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantWorkingDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TenantWorkingDays",
                columns: table => new
                {
                    TenantWorkingDayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsWorkingDay = table.Column<bool>(type: "bit", nullable: false),
                    DefaultStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DefaultEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantWorkingDays", x => x.TenantWorkingDayId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantWorkingDays_TenantId_DayOfWeek",
                table: "TenantWorkingDays",
                columns: new[] { "TenantId", "DayOfWeek" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO TenantWorkingDays (TenantId, DayOfWeek, IsWorkingDay, DefaultStartTime, DefaultEndTime, BreakMinutes)
                SELECT t.TenantId, days.DayOfWeek,
                    CAST(CASE WHEN settings.Value IS NULL OR settings.Value = '' OR CHARINDEX(days.DayName, settings.Value) > 0 THEN 1 ELSE 0 END AS bit),
                    CAST('08:00:00' AS time), CAST('17:00:00' AS time), 60
                FROM Tenants t
                CROSS JOIN (VALUES
                    (0, 'Sunday'), (1, 'Monday'), (2, 'Tuesday'), (3, 'Wednesday'),
                    (4, 'Thursday'), (5, 'Friday'), (6, 'Saturday')
                ) days(DayOfWeek, DayName)
                OUTER APPLY (
                    SELECT TOP 1 Value
                    FROM TenantSettings
                    WHERE TenantId = t.TenantId AND [Key] = 'WorkingWeek'
                ) settings;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TenantWorkingDays");
        }
    }
}
