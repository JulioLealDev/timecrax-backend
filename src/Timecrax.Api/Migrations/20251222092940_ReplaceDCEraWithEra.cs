using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDCEraWithEra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DCEra",
                schema: "app",
                table: "event_cards");

            migrationBuilder.AddColumn<string>(
                name: "Era",
                schema: "app",
                table: "event_cards",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "DC");

            migrationBuilder.AddCheckConstraint(
                name: "ck_event_cards_era",
                schema: "app",
                table: "event_cards",
                sql: "\"Era\" IN ('AC', 'DC')");

            migrationBuilder.Sql(
                "ALTER TABLE app.event_cards ALTER COLUMN \"Era\" DROP DEFAULT;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_event_cards_era",
                schema: "app",
                table: "event_cards");

            migrationBuilder.DropColumn(
                name: "Era",
                schema: "app",
                table: "event_cards");

            migrationBuilder.AddColumn<bool>(
                name: "DCEra",
                schema: "app",
                table: "event_cards",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
