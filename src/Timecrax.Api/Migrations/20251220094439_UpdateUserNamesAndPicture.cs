using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserNamesAndPicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "app",
                table: "users",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "app",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Picture",
                schema: "app",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "app",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Picture",
                schema: "app",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "app",
                table: "users",
                newName: "FullName");
        }
    }
}
