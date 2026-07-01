using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Enterprise.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Role : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Enterprise_EnterpriseUsers_Role",
                table: "Enterprise_EnterpriseUsers",
                column: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enterprise_EnterpriseUsers_Role",
                table: "Enterprise_EnterpriseUsers");
        }
    }
}
