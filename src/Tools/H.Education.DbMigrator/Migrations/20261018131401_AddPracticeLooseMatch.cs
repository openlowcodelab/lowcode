using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Education.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPracticeLooseMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LooseMatch",
                table: "PracticeItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LooseMatch",
                table: "PracticeItems");
        }
    }
}
