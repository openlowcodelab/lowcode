using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Education.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddTextbookCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Textbooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Stage = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Volume = table.Column<int>(type: "int", nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UnitCount = table.Column<int>(type: "int", nullable: false),
                    UnitLabel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    BookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CourseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Textbooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeachingResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TextbookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitSort = table.Column<int>(type: "int", nullable: false),
                    Lesson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingResources_Textbooks_TextbookId",
                        column: x => x.TextbookId,
                        principalTable: "Textbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingResources_TextbookId",
                table: "TeachingResources",
                column: "TextbookId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingResources_TextbookId_UnitSort_Type",
                table: "TeachingResources",
                columns: new[] { "TextbookId", "UnitSort", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingResources_Type",
                table: "TeachingResources",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Textbooks_Grade",
                table: "Textbooks",
                column: "Grade");

            migrationBuilder.CreateIndex(
                name: "IX_Textbooks_Stage",
                table: "Textbooks",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_Textbooks_Subject",
                table: "Textbooks",
                column: "Subject");

            migrationBuilder.CreateIndex(
                name: "IX_Textbooks_Subject_Version_Grade_Volume",
                table: "Textbooks",
                columns: new[] { "Subject", "Version", "Grade", "Volume" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeachingResources");

            migrationBuilder.DropTable(
                name: "Textbooks");
        }
    }
}
