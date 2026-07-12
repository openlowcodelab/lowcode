using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.SupplyChain.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddInterfaceStandardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestFieldsJson",
                table: "ApiInterfaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseFieldsJson",
                table: "ApiInterfaces",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestFieldsJson",
                table: "ApiInterfaces");

            migrationBuilder.DropColumn(
                name: "ResponseFieldsJson",
                table: "ApiInterfaces");
        }
    }
}
