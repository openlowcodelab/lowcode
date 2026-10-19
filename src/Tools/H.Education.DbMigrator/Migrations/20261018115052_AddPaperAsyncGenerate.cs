using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Education.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPaperAsyncGenerate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GenerateError",
                table: "ExamPapers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenerateInput",
                table: "ExamPapers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenerateState",
                table: "ExamPapers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExamPapers_GenerateState",
                table: "ExamPapers",
                column: "GenerateState");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamPapers_GenerateState",
                table: "ExamPapers");

            migrationBuilder.DropColumn(
                name: "GenerateError",
                table: "ExamPapers");

            migrationBuilder.DropColumn(
                name: "GenerateInput",
                table: "ExamPapers");

            migrationBuilder.DropColumn(
                name: "GenerateState",
                table: "ExamPapers");
        }
    }
}
