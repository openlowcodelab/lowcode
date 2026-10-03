using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class WorkbenchPlugin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PluginIds",
                table: "Agent",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkbenchPlugin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PluginKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PluginName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Custom"),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SkillKeys = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkbenchPlugin", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkbenchPlugin_PluginKey",
                table: "WorkbenchPlugin",
                column: "PluginKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkbenchPlugin_Source",
                table: "WorkbenchPlugin",
                column: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkbenchPlugin");

            migrationBuilder.DropColumn(
                name: "PluginIds",
                table: "Agent");
        }
    }
}
