using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Testing.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Case",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "8000000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateId = table.Column<long>(type: "bigint", nullable: true),
                    CaseName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false),
                    LevelsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TagsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TestDataJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastExecutionResult = table.Column<int>(type: "int", nullable: true),
                    LastExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Case", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseCategory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "7000000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseExecutionRecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "9000000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    TestCaseId = table.Column<long>(type: "bigint", nullable: false),
                    EnvironmentId = table.Column<long>(type: "bigint", nullable: false),
                    CaseName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EnvironmentName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Duration = table.Column<long>(type: "bigint", nullable: false),
                    TotalSteps = table.Column<int>(type: "int", nullable: false),
                    SuccessSteps = table.Column<int>(type: "int", nullable: false),
                    FailedSteps = table.Column<int>(type: "int", nullable: false),
                    SkippedSteps = table.Column<int>(type: "int", nullable: false),
                    StepRecordsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExecutedBy = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: true),
                    EnvironmentSnapshotJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseExecutionRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseStep",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "8500000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CaseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExpectedResult = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ApiConfigJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    UiConfigJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ScriptConfigJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseStep", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "100000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KnowledgeBaseId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEnv",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "3000000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    VariablesJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    HeadersJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ServiceConfigsJson = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEnv", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectService",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "2000000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectService", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Case_ProjectId",
                table: "Case",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseCategory_ProjectId",
                table: "CaseCategory",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseExecutionRecord_TestCaseId",
                table: "CaseExecutionRecord",
                column: "TestCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseStep_CaseId",
                table: "CaseStep",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_TenantId",
                table: "Project",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEnv_ProjectId",
                table: "ProjectEnv",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_ProjectId",
                table: "ProjectService",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Case");

            migrationBuilder.DropTable(
                name: "CaseCategory");

            migrationBuilder.DropTable(
                name: "CaseExecutionRecord");

            migrationBuilder.DropTable(
                name: "CaseStep");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "ProjectEnv");

            migrationBuilder.DropTable(
                name: "ProjectService");
        }
    }
}
