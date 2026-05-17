using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.SystemManagement.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationBusinesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BusinessCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_NotificationBusinesses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationMethodConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MethodType = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebhookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SmsTemplateId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_NotificationMethodConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationMethodConfigs_NotificationBusinesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "NotificationBusinesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationBusinesses_BusinessCode",
                table: "NotificationBusinesses",
                column: "BusinessCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationBusinesses_BusinessName",
                table: "NotificationBusinesses",
                column: "BusinessName");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMethodConfigs_BusinessId",
                table: "NotificationMethodConfigs",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationMethodConfigs");

            migrationBuilder.DropTable(
                name: "NotificationBusinesses");
        }
    }
}
