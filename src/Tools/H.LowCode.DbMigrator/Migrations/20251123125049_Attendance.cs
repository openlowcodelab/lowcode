using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.LowCode.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Attendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_leave_request",
                columns: table => new
                {
                    f_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    f_userid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    f_deptid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_leave_type = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    f_start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_end_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_days = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    f_reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_status = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    f_approver = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_approve_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    f_approve_remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_create_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_leave_request", x => x.f_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_overtime_request",
                columns: table => new
                {
                    f_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    f_userid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    f_deptid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_overtime_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_end_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    f_hours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    f_overtime_type = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    f_reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_status = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    f_approver = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_approve_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    f_approve_remark = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    f_create_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifierId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_overtime_request", x => x.f_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_leave_request");

            migrationBuilder.DropTable(
                name: "tb_overtime_request");
        }
    }
}
