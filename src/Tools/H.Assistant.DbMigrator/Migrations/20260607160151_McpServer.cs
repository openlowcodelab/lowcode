using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Assistant.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class McpServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "McpServer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransportType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AuthToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Headers = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpServer", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_McpServer_IsEnabled",
                table: "McpServer",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_McpServer_Name",
                table: "McpServer",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McpServer");
        }
    }
}
