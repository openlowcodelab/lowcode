using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.LowCode.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_test1",
                columns: table => new
                {
                    f_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    f_field1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field2 = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    f_field3 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    f_field4 = table.Column<bool>(type: "bit", nullable: false),
                    f_field5 = table.Column<int>(type: "int", nullable: true),
                    f_field6 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field7 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field8 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field9 = table.Column<bool>(type: "bit", nullable: true),
                    f_field10 = table.Column<DateTime>(type: "datetime2", nullable: true),
                    f_field11 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field12 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUser = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_test1", x => x.f_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_test1");
        }
    }
}
