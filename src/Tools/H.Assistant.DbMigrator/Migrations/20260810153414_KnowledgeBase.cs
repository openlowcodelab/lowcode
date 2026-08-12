using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Assistant.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class KnowledgeBase : Migration
    {
        /// <summary> 默认知识库固定 ID（存量知识节点迁移目标） </summary>
        private const string DefaultKnowledgeBaseId = "B7E2A4C1-3D5F-4E6A-8B9C-0D1E2F3A4B5C";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KnowledgeNode_OwnerType_ParentId",
                table: "KnowledgeNode");

            migrationBuilder.AddColumn<Guid>(
                name: "KnowledgeBaseId",
                table: "KnowledgeNode",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KnowledgeBase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeBase", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeNode_OwnerType_KnowledgeBaseId_ParentId",
                table: "KnowledgeNode",
                columns: new[] { "OwnerType", "KnowledgeBaseId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBase_Name",
                table: "KnowledgeBase",
                column: "Name",
                unique: true);

            // 创建默认知识库，并将存量知识节点迁移到该库
            migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM [KnowledgeBase] WHERE [Id] = '{DefaultKnowledgeBaseId}')
    INSERT INTO [KnowledgeBase] ([Id], [Name], [Description], [SortOrder], [CreationTime])
    VALUES ('{DefaultKnowledgeBaseId}', N'默认知识库', N'系统默认知识库', 0, SYSUTCDATETIME());

UPDATE [KnowledgeNode]
SET [KnowledgeBaseId] = '{DefaultKnowledgeBaseId}'
WHERE [OwnerType] = 'Knowledge' AND [KnowledgeBaseId] IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE [KnowledgeNode] SET [KnowledgeBaseId] = NULL WHERE [KnowledgeBaseId] = '{DefaultKnowledgeBaseId}';");

            migrationBuilder.DropTable(
                name: "KnowledgeBase");

            migrationBuilder.DropIndex(
                name: "IX_KnowledgeNode_OwnerType_KnowledgeBaseId_ParentId",
                table: "KnowledgeNode");

            migrationBuilder.DropColumn(
                name: "KnowledgeBaseId",
                table: "KnowledgeNode");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeNode_OwnerType_ParentId",
                table: "KnowledgeNode",
                columns: new[] { "OwnerType", "ParentId" });
        }
    }
}
