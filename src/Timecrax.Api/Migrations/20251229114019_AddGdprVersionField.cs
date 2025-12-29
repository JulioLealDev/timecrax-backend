using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGdprVersionField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GdprAccepted",
                schema: "app",
                table: "users");

            migrationBuilder.AddColumn<int>(
                name: "GdprVersion",
                schema: "app",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "app",
                table: "gdpr",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GdprVersion",
                schema: "app",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "app",
                table: "gdpr");

            migrationBuilder.AddColumn<string>(
                name: "GdprAccepted",
                schema: "app",
                table: "users",
                type: "text",
                nullable: true);
        }
    }
}
