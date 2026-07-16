using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Approval.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalCategories_Name",
                table: "ApprovalCategories",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalCategories");
        }
    }
}
