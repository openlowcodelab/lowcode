using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Testing.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Add_Settings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 清理旧版单列设置表（来自已移除的迁移），若不存在则为无操作
            migrationBuilder.Sql(@"IF OBJECT_ID(N'TestingSettings', N'U') IS NOT NULL DROP TABLE [TestingSettings];");
            migrationBuilder.Sql(@"DELETE FROM [__EFMigrationsHistory] WHERE MigrationId = N'20260830101017_Add_TestingSettings';");

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Key = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
