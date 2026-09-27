using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H.Workbench.DbMigrator.Migrations
{
    /// <inheritdoc />
    public partial class ArtifactReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "Artifact",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "Artifact",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Artifact",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerId",
                table: "Artifact",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artifact_ReviewStatus",
                table: "Artifact",
                column: "ReviewStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Artifact_ReviewStatus",
                table: "Artifact");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "Artifact");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Artifact");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Artifact");

            migrationBuilder.DropColumn(
                name: "ReviewerId",
                table: "Artifact");
        }
    }
}
