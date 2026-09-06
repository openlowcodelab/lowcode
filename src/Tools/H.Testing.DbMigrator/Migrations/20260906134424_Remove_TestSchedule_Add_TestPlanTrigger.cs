using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Testing.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Remove_TestSchedule_Add_TestPlanTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestSchedule");

            migrationBuilder.AddColumn<string>(
                name: "CronExpression",
                table: "TestPlan",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EnvId",
                table: "TestPlan",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "TestPlan",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastExecutionStatus",
                table: "TestPlan",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastExecutionTime",
                table: "TestPlan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TriggerType",
                table: "TestPlan",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CronExpression",
                table: "TestPlan");

            migrationBuilder.DropColumn(
                name: "EnvId",
                table: "TestPlan");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "TestPlan");

            migrationBuilder.DropColumn(
                name: "LastExecutionStatus",
                table: "TestPlan");

            migrationBuilder.DropColumn(
                name: "LastExecutionTime",
                table: "TestPlan");

            migrationBuilder.DropColumn(
                name: "TriggerType",
                table: "TestPlan");

            migrationBuilder.CreateTable(
                name: "TestSchedule",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "9500000, 1"),
                    CaseScope = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CronExpression = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnvId = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastExecutionStatus = table.Column<int>(type: "int", nullable: true),
                    LastExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    SelectedCaseIdsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSchedule", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestSchedule_ProjectId",
                table: "TestSchedule",
                column: "ProjectId");
        }
    }
}
