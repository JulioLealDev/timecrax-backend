using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEraToBCAndAD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_event_cards_era",
                schema: "app",
                table: "event_cards");

            // Convert existing data from AC/DC to BC/AD
            migrationBuilder.Sql("UPDATE app.event_cards SET \"Era\" = 'BC' WHERE \"Era\" = 'AC'");
            migrationBuilder.Sql("UPDATE app.event_cards SET \"Era\" = 'AD' WHERE \"Era\" = 'DC'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_event_cards_era",
                schema: "app",
                table: "event_cards",
                sql: "\"Era\" IN ('BC', 'AD')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_event_cards_era",
                schema: "app",
                table: "event_cards");

            // Revert data from BC/AD to AC/DC
            migrationBuilder.Sql("UPDATE app.event_cards SET \"Era\" = 'AC' WHERE \"Era\" = 'BC'");
            migrationBuilder.Sql("UPDATE app.event_cards SET \"Era\" = 'DC' WHERE \"Era\" = 'AD'");

            migrationBuilder.AddCheckConstraint(
                name: "ck_event_cards_era",
                schema: "app",
                table: "event_cards",
                sql: "\"Era\" IN ('AC', 'DC')");
        }
    }
}
