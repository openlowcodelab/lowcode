using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.BackgroundTask.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackgroundJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TriggerKind = table.Column<int>(type: "int", nullable: false),
                    ExecuteType = table.Column<int>(type: "int", nullable: false),
                    CronExpression = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApiUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApiHttpMethod = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    ApiHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SqlConnectionString = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SqlStatement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HangfireJobId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastExecutionStatus = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobExecutionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExecuteType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutionRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_ExecuteType",
                table: "BackgroundJobs",
                column: "ExecuteType");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_IsEnabled",
                table: "BackgroundJobs",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_Name",
                table: "BackgroundJobs",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundJobs_TriggerKind",
                table: "BackgroundJobs",
                column: "TriggerKind");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionRecords_JobId",
                table: "JobExecutionRecords",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionRecords_StartTime",
                table: "JobExecutionRecords",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionRecords_Status",
                table: "JobExecutionRecords",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackgroundJobs");

            migrationBuilder.DropTable(
                name: "JobExecutionRecords");
        }
    }
}
