using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMedalsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ranks",
                schema: "app");

            migrationBuilder.CreateTable(
                name: "medals",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: false),
                    MinScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medals", x => x.Id);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medals",
                schema: "app");

            migrationBuilder.CreateTable(
                name: "ranks",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: false),
                    MinScore = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ranks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ranks_MinScore",
                schema: "app",
                table: "ranks",
                column: "MinScore",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ranks_Name",
                schema: "app",
                table: "ranks",
                column: "Name",
                unique: true);
        }
    }
}
