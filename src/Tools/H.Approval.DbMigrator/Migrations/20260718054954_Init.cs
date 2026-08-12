using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Approval.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalDefinitions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DefinitionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WhoCanStart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecifiedStarters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecifiedAdmins = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalInstances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DefinitionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DefinitionName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CurrentNodeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CurrentNodeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    VariablesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalTasks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InstanceId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApprovalName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    InstanceTitle = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    NodeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NodeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    AssigneeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AssigneeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalTasks_ApprovalInstances_InstanceId",
                        column: x => x.InstanceId,
                        principalTable: "ApprovalInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalCategories_Name",
                table: "ApprovalCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTasks_AssigneeId",
                table: "ApprovalTasks",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTasks_InstanceId",
                table: "ApprovalTasks",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTasks_NodeId",
                table: "ApprovalTasks",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTasks_Status",
                table: "ApprovalTasks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalCategories");

            migrationBuilder.DropTable(
                name: "ApprovalDefinitions");

            migrationBuilder.DropTable(
                name: "ApprovalTasks");

            migrationBuilder.DropTable(
                name: "ApprovalInstances");
        }
    }
}
