using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AgentWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkflowName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Steps = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_AgentType_WorkflowName",
                table: "Workflow",
                columns: new[] { "AgentType", "WorkflowName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_IsEnabled",
                table: "Workflow",
                column: "IsEnabled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workflow");
        }
    }
}
