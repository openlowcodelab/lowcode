using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class RunAcceptanceAndBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                table: "TaskLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                table: "TaskLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Verdict",
                table: "TaskLog",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerdictReason",
                table: "TaskLog",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcceptanceCriteria",
                table: "Task",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                table: "TaskLog");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                table: "TaskLog");

            migrationBuilder.DropColumn(
                name: "Verdict",
                table: "TaskLog");

            migrationBuilder.DropColumn(
                name: "VerdictReason",
                table: "TaskLog");

            migrationBuilder.DropColumn(
                name: "AcceptanceCriteria",
                table: "Task");
        }
    }
}
