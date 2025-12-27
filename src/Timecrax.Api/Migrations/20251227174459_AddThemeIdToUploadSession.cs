using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeIdToUploadSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Resume",
                schema: "app",
                table: "themes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThemeId",
                schema: "app",
                table: "theme_upload_sessions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resume",
                schema: "app",
                table: "themes");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                schema: "app",
                table: "theme_upload_sessions");
        }
    }
}
