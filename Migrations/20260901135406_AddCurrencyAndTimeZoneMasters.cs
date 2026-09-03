using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyAndTimeZoneMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    CurrencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.CurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "TenantCurrencies",
                columns: table => new
                {
                    TenantCurrencyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantCurrencies", x => x.TenantCurrencyId);
                });

            migrationBuilder.CreateTable(
                name: "TimeZones",
                columns: table => new
                {
                    TimeZoneId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeZones", x => x.TimeZoneId);
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "CurrencyId", "Code", "DecimalPlaces", "IsActive", "Name", "Symbol" },
                values: new object[,]
                {
                    { 1, "QAR", 2, true, "Qatari Riyal", "QAR" },
                    { 2, "AED", 2, true, "UAE Dirham", "AED" },
                    { 3, "SAR", 2, true, "Saudi Riyal", "SAR" },
                    { 4, "USD", 2, true, "US Dollar", "$" },
                    { 5, "GBP", 2, true, "Pound Sterling", "£" },
                    { 6, "INR", 2, true, "Indian Rupee", "₹" },
                    { 7, "PKR", 2, true, "Pakistani Rupee", "Rs" }
                });

            migrationBuilder.InsertData(
                table: "TimeZones",
                columns: new[] { "TimeZoneId", "CountryCode", "DisplayName", "IsActive" },
                values: new object[,]
                {
                    { "America/New_York", "US", "United States Eastern (America/New_York)", true },
                    { "Asia/Dubai", "AE", "United Arab Emirates (Asia/Dubai)", true },
                    { "Asia/Karachi", "PK", "Pakistan (Asia/Karachi)", true },
                    { "Asia/Kolkata", "IN", "India (Asia/Kolkata)", true },
                    { "Asia/Qatar", "QA", "Qatar (Asia/Qatar)", true },
                    { "Asia/Riyadh", "SA", "Saudi Arabia (Asia/Riyadh)", true },
                    { "Europe/London", "GB", "United Kingdom (Europe/London)", true },
                    { "UTC", null, "Coordinated Universal Time (UTC)", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantCurrencies_TenantId_CurrencyCode",
                table: "TenantCurrencies",
                columns: new[] { "TenantId", "CurrencyCode" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO TenantCurrencies (TenantId, CurrencyCode, IsEnabled)
                SELECT TenantId, CurrencyCode, CAST(1 AS bit)
                FROM Tenants
                WHERE CurrencyCode IS NOT NULL AND CurrencyCode <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "TenantCurrencies");

            migrationBuilder.DropTable(
                name: "TimeZones");
        }
    }
}
