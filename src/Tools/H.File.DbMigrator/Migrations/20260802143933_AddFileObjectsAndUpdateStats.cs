using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.File.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddFileObjectsAndUpdateStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileCount",
                table: "FileProjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "TotalSize",
                table: "FileProjects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "FileObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FolderPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileObjects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileObjects_ProjectId_FolderPath",
                table: "FileObjects",
                columns: new[] { "ProjectId", "FolderPath" });

            migrationBuilder.CreateIndex(
                name: "IX_FileObjects_ProjectId_Key",
                table: "FileObjects",
                columns: new[] { "ProjectId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileObjects");

            migrationBuilder.DropColumn(
                name: "FileCount",
                table: "FileProjects");

            migrationBuilder.DropColumn(
                name: "TotalSize",
                table: "FileProjects");
        }
    }
}
