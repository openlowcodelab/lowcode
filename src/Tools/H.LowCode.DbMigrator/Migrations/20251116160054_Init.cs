using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.LowCode.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_check",
                columns: table => new
                {
                    f_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    f_userid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    f_deptid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_checktime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_checklocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_checktype = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_check", x => x.f_id);
                });

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
                    f_field8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    f_field9 = table.Column<bool>(type: "bit", nullable: true),
                    f_field10 = table.Column<DateTime>(type: "datetime2", nullable: true),
                    f_field11 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field12 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field13 = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_test1", x => x.f_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_test2",
                columns: table => new
                {
                    f_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    f_field1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_field3 = table.Column<bool>(type: "bit", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_test2", x => x.f_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_test3",
                columns: table => new
                {
                    f_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    f_field1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_test3", x => x.f_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_check");

            migrationBuilder.DropTable(
                name: "tb_test1");

            migrationBuilder.DropTable(
                name: "tb_test2");

            migrationBuilder.DropTable(
                name: "tb_test3");
        }
    }
}
