using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddThemesAchievementsQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_cards_ThemeId",
                schema: "app",
                table: "event_cards");

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                schema: "app",
                table: "event_cards",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                schema: "app",
                table: "event_cards",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DCEra",
                schema: "app",
                table: "event_cards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                schema: "app",
                table: "event_cards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "ck_text_quizzes_correct",
                schema: "app",
                table: "text_quizzes",
                sql: "\"CorrectTextIndex\" BETWEEN 0 AND 3");

            migrationBuilder.AddCheckConstraint(
                name: "ck_image_quizzes_correct",
                schema: "app",
                table: "image_quizzes",
                sql: "\"CorrectImageIndex\" BETWEEN 0 AND 3");

            migrationBuilder.CreateIndex(
                name: "IX_event_cards_ThemeId_CreatedAt",
                schema: "app",
                table: "event_cards",
                columns: new[] { "ThemeId", "CreatedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_event_cards_year",
                schema: "app",
                table: "event_cards",
                sql: "\"Year\" > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_text_quizzes_correct",
                schema: "app",
                table: "text_quizzes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_image_quizzes_correct",
                schema: "app",
                table: "image_quizzes");

            migrationBuilder.DropIndex(
                name: "IX_event_cards_ThemeId_CreatedAt",
                schema: "app",
                table: "event_cards");

            migrationBuilder.DropCheckConstraint(
                name: "ck_event_cards_year",
                schema: "app",
                table: "event_cards");

            migrationBuilder.DropColumn(
                name: "DCEra",
                schema: "app",
                table: "event_cards");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                schema: "app",
                table: "event_cards");

            migrationBuilder.AlterColumn<int>(
                name: "Year",
                schema: "app",
                table: "event_cards",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                schema: "app",
                table: "event_cards",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_event_cards_ThemeId",
                schema: "app",
                table: "event_cards",
                column: "ThemeId");
        }
    }
}
