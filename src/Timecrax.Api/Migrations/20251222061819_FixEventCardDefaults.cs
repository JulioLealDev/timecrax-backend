using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixEventCardDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
        "ALTER TABLE app.event_cards ALTER COLUMN \"Year\" DROP DEFAULT;"
            );

            migrationBuilder.Sql(@"
        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1
                FROM pg_constraint c
                JOIN pg_class t ON c.conrelid = t.oid
                JOIN pg_namespace n ON n.oid = t.relnamespace
                WHERE c.conname = 'ck_event_cards_year'
                AND n.nspname = 'app'
                AND t.relname = 'event_cards'
            ) THEN
                ALTER TABLE app.event_cards
                ADD CONSTRAINT ck_event_cards_year CHECK (""Year"" > 0);
            END IF;
        END $$;
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.DropCheckConstraint(
            name: "ck_event_cards_year",
            schema: "app",
            table: "event_cards"
            );

            // Restaura o DEFAULT (não recomendado, mas reversível)
            migrationBuilder.Sql(
                "ALTER TABLE app.event_cards ALTER COLUMN \"Year\" SET DEFAULT 0;"
            );
        }
    }
}
