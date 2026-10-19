using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Education.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicDrillAndUnitTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlankCount",
                table: "PracticeItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "ExamPapers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Textbook",
                table: "ExamPapers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ExamPapers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlankCount",
                table: "PracticeItems");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "ExamPapers");

            migrationBuilder.DropColumn(
                name: "Textbook",
                table: "ExamPapers");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ExamPapers");
        }
    }
}
