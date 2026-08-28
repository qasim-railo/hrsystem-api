using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step33BackfillTenantDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Tenants
                SET CountryCode = CASE WHEN CountryCode = '' THEN UPPER(ISNULL(NULLIF(Country, ''), 'QA')) ELSE CountryCode END,
                    CurrencyCode = CASE WHEN CurrencyCode = '' THEN UPPER(ISNULL(NULLIF(Currency, ''), 'QAR')) ELSE CurrencyCode END,
                    TimeZoneId = CASE WHEN TimeZoneId = '' THEN ISNULL(NULLIF(TimeZone, ''), 'Asia/Qatar') ELSE TimeZoneId END,
                    DateFormat = CASE WHEN DateFormat = '' THEN 'dd/MM/yyyy' ELSE DateFormat END,
                    NumberFormat = CASE WHEN NumberFormat = '' THEN 'en-QA' ELSE NumberFormat END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
