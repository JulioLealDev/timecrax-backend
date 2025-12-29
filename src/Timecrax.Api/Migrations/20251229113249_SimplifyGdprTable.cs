using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyGdprTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_gdpr",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropIndex(
                name: "IX_gdpr_LastUpdate",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropIndex(
                name: "IX_gdpr_Version",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropColumn(
                name: "LastUpdate",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "app",
                table: "gdpr");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "app",
                table: "gdpr",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gdpr",
                schema: "app",
                table: "gdpr",
                column: "Language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_gdpr",
                schema: "app",
                table: "gdpr");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "app",
                table: "gdpr");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "app",
                table: "gdpr",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "app",
                table: "gdpr",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<bool>(
                name: "LastUpdate",
                schema: "app",
                table: "gdpr",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "app",
                table: "gdpr",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Version",
                schema: "app",
                table: "gdpr",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_gdpr",
                schema: "app",
                table: "gdpr",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_gdpr_LastUpdate",
                schema: "app",
                table: "gdpr",
                column: "LastUpdate",
                unique: true,
                filter: "\"LastUpdate\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_gdpr_Version",
                schema: "app",
                table: "gdpr",
                column: "Version",
                unique: true);
        }
    }
}
