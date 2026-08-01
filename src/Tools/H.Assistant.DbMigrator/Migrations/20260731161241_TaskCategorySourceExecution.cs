using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Assistant.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class TaskCategorySourceExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Task",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExecutionMode",
                table: "Task",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Auto");

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "Task",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Prompt");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowContent",
                table: "Task",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_Category",
                table: "Task",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Task_Category",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "ExecutionMode",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "WorkflowContent",
                table: "Task");
        }
    }
}
