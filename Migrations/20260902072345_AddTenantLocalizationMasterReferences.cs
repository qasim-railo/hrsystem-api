using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantLocalizationMasterReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultCountryId",
                table: "Tenants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultCurrencyId",
                table: "Tenants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultTimeZoneId",
                table: "Tenants",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "TenantCurrencies",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO Countries (Code, Name, IsActive)
                SELECT DISTINCT t.CountryCode, t.CountryCode, CAST(1 AS bit)
                FROM Tenants t
                WHERE t.CountryCode <> '' AND NOT EXISTS (SELECT 1 FROM Countries c WHERE c.Code = t.CountryCode);

                INSERT INTO Currencies (Code, Name, Symbol, DecimalPlaces, IsActive)
                SELECT DISTINCT source.CurrencyCode, source.CurrencyCode, source.CurrencyCode, 2, CAST(1 AS bit)
                FROM (
                    SELECT CurrencyCode FROM Tenants
                    UNION
                    SELECT CurrencyCode FROM TenantCurrencies
                ) source
                WHERE source.CurrencyCode <> '' AND NOT EXISTS (SELECT 1 FROM Currencies c WHERE c.Code = source.CurrencyCode);

                INSERT INTO TimeZones (TimeZoneId, DisplayName, CountryCode, IsActive)
                SELECT DISTINCT t.TimeZoneId, t.TimeZoneId, NULL, CAST(1 AS bit)
                FROM Tenants t
                WHERE t.TimeZoneId <> '' AND NOT EXISTS (SELECT 1 FROM TimeZones z WHERE z.TimeZoneId = t.TimeZoneId);

                UPDATE t
                SET DefaultCountryId = c.CountryId,
                    DefaultCurrencyId = currency.CurrencyId,
                    DefaultTimeZoneId = z.TimeZoneId
                FROM Tenants t
                INNER JOIN Countries c ON c.Code = t.CountryCode
                INNER JOIN Currencies currency ON currency.Code = t.CurrencyCode
                INNER JOIN TimeZones z ON z.TimeZoneId = t.TimeZoneId;

                UPDATE tenantCurrency
                SET CurrencyId = currency.CurrencyId
                FROM TenantCurrencies tenantCurrency
                INNER JOIN Currencies currency ON currency.Code = tenantCurrency.CurrencyCode;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultCountryId",
                table: "Tenants",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultCurrencyId",
                table: "Tenants",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultTimeZoneId",
                table: "Tenants",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrencyId",
                table: "TenantCurrencies",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_TenantCurrencies_TenantId_CurrencyCode",
                table: "TenantCurrencies");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "TenantCurrencies");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DefaultCountryId",
                table: "Tenants",
                column: "DefaultCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DefaultCurrencyId",
                table: "Tenants",
                column: "DefaultCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DefaultTimeZoneId",
                table: "Tenants",
                column: "DefaultTimeZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantCurrencies_CurrencyId",
                table: "TenantCurrencies",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantCurrencies_TenantId_CurrencyId",
                table: "TenantCurrencies",
                columns: new[] { "TenantId", "CurrencyId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantCurrencies_Currencies_CurrencyId",
                table: "TenantCurrencies",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Countries_DefaultCountryId",
                table: "Tenants",
                column: "DefaultCountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Currencies_DefaultCurrencyId",
                table: "Tenants",
                column: "DefaultCurrencyId",
                principalTable: "Currencies",
                principalColumn: "CurrencyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TimeZones_DefaultTimeZoneId",
                table: "Tenants",
                column: "DefaultTimeZoneId",
                principalTable: "TimeZones",
                principalColumn: "TimeZoneId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantCurrencies_Currencies_CurrencyId",
                table: "TenantCurrencies");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Countries_DefaultCountryId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Currencies_DefaultCurrencyId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_TimeZones_DefaultTimeZoneId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_DefaultCountryId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_DefaultCurrencyId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_DefaultTimeZoneId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_TenantCurrencies_CurrencyId",
                table: "TenantCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_TenantCurrencies_TenantId_CurrencyId",
                table: "TenantCurrencies");

            migrationBuilder.DropColumn(
                name: "DefaultCountryId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DefaultCurrencyId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DefaultTimeZoneId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "TenantCurrencies");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "TenantCurrencies",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TenantCurrencies_TenantId_CurrencyCode",
                table: "TenantCurrencies",
                columns: new[] { "TenantId", "CurrencyCode" },
                unique: true);
        }
    }
}
