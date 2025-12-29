using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGdprTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GdprAccepted",
                schema: "app",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gdpr",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Terms = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    LastUpdate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gdpr", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gdpr_LastUpdate",
                schema: "app",
                table: "gdpr",
                column: "LastUpdate",
                unique: true,
                filter: "\"LastUpdate\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_gdpr_Version",
                schema: "app",
                table: "gdpr",
                column: "Version",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gdpr",
                schema: "app");

            migrationBuilder.DropColumn(
                name: "GdprAccepted",
                schema: "app",
                table: "users");
        }
    }
}
