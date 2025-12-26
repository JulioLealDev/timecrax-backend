using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueOpenThemeUploadSessionPerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_theme_upload_sessions_UserId",
                schema: "app",
                table: "theme_upload_sessions");

            migrationBuilder.CreateIndex(
                name: "UX_ThemeUploadSessions_User_OpenSession",
                schema: "app",
                table: "theme_upload_sessions",
                column: "UserId",
                unique: true,
                filter: "\"IsClosed\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ThemeUploadSessions_User_OpenSession",
                schema: "app",
                table: "theme_upload_sessions");

            migrationBuilder.CreateIndex(
                name: "IX_theme_upload_sessions_UserId",
                schema: "app",
                table: "theme_upload_sessions",
                column: "UserId");
        }
    }
}
