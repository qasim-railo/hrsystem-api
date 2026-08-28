using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class Step36ImportCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[ImportJobs]', N'U') IS NULL
                CREATE TABLE [ImportJobs] (
                    [ImportJobId] int NOT NULL IDENTITY(1,1) CONSTRAINT [PK_ImportJobs] PRIMARY KEY,
                    [TenantId] int NOT NULL, [UserId] int NOT NULL,
                    [EntityType] nvarchar(max) NOT NULL, [FileName] nvarchar(max) NOT NULL,
                    [Status] nvarchar(max) NOT NULL, [TotalRows] int NOT NULL,
                    [ValidRows] int NOT NULL, [ImportedRows] int NOT NULL, [ErrorRows] int NOT NULL,
                    [ErrorsJson] nvarchar(max) NOT NULL, [RowsJson] nvarchar(max) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID(N'[ImportJobs]', N'U') IS NOT NULL DROP TABLE [ImportJobs];");
        }
    }
}
