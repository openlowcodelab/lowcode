using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Approval.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFormJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormJson",
                table: "ApprovalDefinitions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormJson",
                table: "ApprovalDefinitions");
        }
    }
}
