using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Education.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeAudio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudioText",
                table: "PracticeItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioText",
                table: "PracticeItems");
        }
    }
}
