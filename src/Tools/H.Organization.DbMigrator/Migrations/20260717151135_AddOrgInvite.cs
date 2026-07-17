using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Organization.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgInvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organization_Invites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberType = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Invites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organization_Invites_Organization_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization_Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Invites_OrganizationId",
                table: "Organization_Invites",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Invites_Token",
                table: "Organization_Invites",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organization_Invites");
        }
    }
}
