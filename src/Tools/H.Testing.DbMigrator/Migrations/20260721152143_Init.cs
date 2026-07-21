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
                name: "TestingEnvironmentServiceConfigs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnvironmentId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectServiceId = table.Column<long>(type: "bigint", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_TestingEnvironmentServiceConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestingExecutionRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    TestCaseId = table.Column<long>(type: "bigint", nullable: false),
                    EnvironmentId = table.Column<long>(type: "bigint", nullable: false),
                    TestCaseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EnvironmentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Duration = table.Column<long>(type: "bigint", nullable: false),
                    TotalSteps = table.Column<int>(type: "int", nullable: false),
                    SuccessSteps = table.Column<int>(type: "int", nullable: false),
                    FailedSteps = table.Column<int>(type: "int", nullable: false),
                    SkippedSteps = table.Column<int>(type: "int", nullable: false),
                    StepRecordsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EnvironmentSnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_TestingExecutionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestingProjectCaseCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_TestingProjectCaseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestingProjectCases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    TemplateId = table.Column<long>(type: "bigint", nullable: true),
                    CaseNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false),
                    LevelsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TagsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastExecutionResult = table.Column<int>(type: "int", nullable: true),
                    LastExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_TestingProjectCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestingProjectEnvironments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    VariablesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatabaseConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_TestingProjectEnvironments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestingProjects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EnvironmentIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_TestingProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestingProjectServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
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
                    table.PrimaryKey("PK_TestingProjectServices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestingEnvironmentServiceConfigs_EnvironmentId",
                table: "TestingEnvironmentServiceConfigs",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingEnvironmentServiceConfigs_ProjectServiceId",
                table: "TestingEnvironmentServiceConfigs",
                column: "ProjectServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingEnvironmentServiceConfigs_TenantId",
                table: "TestingEnvironmentServiceConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingExecutionRecords_EnvironmentId",
                table: "TestingExecutionRecords",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingExecutionRecords_ProjectId",
                table: "TestingExecutionRecords",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingExecutionRecords_TenantId",
                table: "TestingExecutionRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingExecutionRecords_TestCaseId",
                table: "TestingExecutionRecords",
                column: "TestCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectCaseCategories_ParentId",
                table: "TestingProjectCaseCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectCaseCategories_ProjectId",
                table: "TestingProjectCaseCategories",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectCaseCategories_TenantId",
                table: "TestingProjectCaseCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectCases_CategoryId",
                table: "TestingProjectCases",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectCases_ProjectId",
                table: "TestingProjectCases",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectCases_TenantId",
                table: "TestingProjectCases",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectEnvironments_ProjectId",
                table: "TestingProjectEnvironments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectEnvironments_TenantId",
                table: "TestingProjectEnvironments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjects_TenantId",
                table: "TestingProjects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectServices_ProjectId",
                table: "TestingProjectServices",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestingProjectServices_TenantId",
                table: "TestingProjectServices",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestingEnvironmentServiceConfigs");

            migrationBuilder.DropTable(
                name: "TestingExecutionRecords");

            migrationBuilder.DropTable(
                name: "TestingProjectCaseCategories");

            migrationBuilder.DropTable(
                name: "TestingProjectCases");

            migrationBuilder.DropTable(
                name: "TestingProjectEnvironments");

            migrationBuilder.DropTable(
                name: "TestingProjects");

            migrationBuilder.DropTable(
                name: "TestingProjectServices");
        }
    }
}
