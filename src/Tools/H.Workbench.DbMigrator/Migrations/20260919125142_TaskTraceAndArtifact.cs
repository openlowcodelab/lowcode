using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class TaskTraceAndArtifact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalState",
                table: "TaskLog",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtifactCount",
                table: "TaskLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StepCount",
                table: "TaskLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Artifact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Repo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RepoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommitHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PushResult = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ToolCallId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Iteration = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artifact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskExecutionStep",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Iteration = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ToolName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SkillName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ToolCallId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Arguments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsError = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApproverId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    WaitMs = table.Column<int>(type: "int", nullable: true),
                    Truncated = table.Column<bool>(type: "bit", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskExecutionStep", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artifact_Kind",
                table: "Artifact",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Artifact_TaskId_CreationTime",
                table: "Artifact",
                columns: new[] { "TaskId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Artifact_TaskLogId",
                table: "Artifact",
                column: "TaskLogId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskExecutionStep_TaskId_StartedAt",
                table: "TaskExecutionStep",
                columns: new[] { "TaskId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskExecutionStep_TaskLogId_Iteration_Seq",
                table: "TaskExecutionStep",
                columns: new[] { "TaskLogId", "Iteration", "Seq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Artifact");

            migrationBuilder.DropTable(
                name: "TaskExecutionStep");

            migrationBuilder.DropColumn(
                name: "ApprovalState",
                table: "TaskLog");

            migrationBuilder.DropColumn(
                name: "ArtifactCount",
                table: "TaskLog");

            migrationBuilder.DropColumn(
                name: "StepCount",
                table: "TaskLog");
        }
    }
}
