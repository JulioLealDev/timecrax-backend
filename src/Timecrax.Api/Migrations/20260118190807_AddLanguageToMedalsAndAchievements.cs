using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageToMedalsAndAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_medals_MinScore",
                schema: "app",
                table: "medals");

            migrationBuilder.DropIndex(
                name: "IX_medals_Name",
                schema: "app",
                table: "medals");

            migrationBuilder.DropIndex(
                name: "IX_achievements_Name",
                schema: "app",
                table: "achievements");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "app",
                table: "medals",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "app",
                table: "achievements",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_medals_MinScore_Language",
                schema: "app",
                table: "medals",
                columns: new[] { "MinScore", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medals_Name_Language",
                schema: "app",
                table: "medals",
                columns: new[] { "Name", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_achievements_Name_Language",
                schema: "app",
                table: "achievements",
                columns: new[] { "Name", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_medals_MinScore_Language",
                schema: "app",
                table: "medals");

            migrationBuilder.DropIndex(
                name: "IX_medals_Name_Language",
                schema: "app",
                table: "medals");

            migrationBuilder.DropIndex(
                name: "IX_achievements_Name_Language",
                schema: "app",
                table: "achievements");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "app",
                table: "medals");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "app",
                table: "achievements");

            migrationBuilder.CreateIndex(
                name: "IX_medals_MinScore",
                schema: "app",
                table: "medals",
                column: "MinScore",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medals_Name",
                schema: "app",
                table: "medals",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_achievements_Name",
                schema: "app",
                table: "achievements",
                column: "Name",
                unique: true);
        }
    }
}
