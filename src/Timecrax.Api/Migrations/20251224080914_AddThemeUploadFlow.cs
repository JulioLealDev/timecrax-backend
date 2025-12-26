using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeUploadFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Image",
                schema: "app",
                table: "themes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_cards_ThemeId_OrderIndex",
                schema: "app",
                table: "event_cards",
                columns: new[] { "ThemeId", "OrderIndex" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_event_cards_orderindex",
                schema: "app",
                table: "event_cards",
                sql: "\"OrderIndex\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_cards_ThemeId_OrderIndex",
                schema: "app",
                table: "event_cards");

            migrationBuilder.DropCheckConstraint(
                name: "ck_event_cards_orderindex",
                schema: "app",
                table: "event_cards");

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                schema: "app",
                table: "themes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
