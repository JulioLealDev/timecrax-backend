using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeToThemes : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resume",
                schema: "app",
                table: "themes");
        }
    }
}
