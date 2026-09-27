using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class ApprovalRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalRule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AgentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ToolPattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ArgPattern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Effect = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Require"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRule_AgentType",
                table: "ApprovalRule",
                column: "AgentType");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRule_IsEnabled_Priority",
                table: "ApprovalRule",
                columns: new[] { "IsEnabled", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalRule");
        }
    }
}
