using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class WorkbenchProjectResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectResource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Config = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectResource", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectResource_ProjectId",
                table: "ProjectResource",
                column: "ProjectId");

            // 存量项目的仓库地址/默认分支搬迁为 code 类型资源（CHAR(34) 即双引号，拼出 {"branch":"..."}）
            migrationBuilder.Sql(@"
INSERT INTO [ProjectResource] ([Id], [ProjectId], [ResourceType], [Name], [Url], [Config], [CreationTime])
SELECT NEWID(), p.[Id], 'code', N'代码仓库', p.[RepoUrl],
       CASE WHEN p.[DefaultBranch] IS NOT NULL AND p.[DefaultBranch] <> ''
            THEN N'{' + CHAR(34) + 'branch' + CHAR(34) + ':' + CHAR(34) + p.[DefaultBranch] + CHAR(34) + N'}'
            ELSE NULL END,
       GETUTCDATE()
FROM [Project] p
WHERE p.[RepoUrl] IS NOT NULL AND p.[RepoUrl] <> '';");

            migrationBuilder.DropColumn(
                name: "DefaultBranch",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "RepoUrl",
                table: "Project");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectResource");

            migrationBuilder.AddColumn<string>(
                name: "DefaultBranch",
                table: "Project",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepoUrl",
                table: "Project",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
