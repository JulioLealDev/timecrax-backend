using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timecrax.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddThemesAchievementsAndQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Score",
                schema: "app",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "achievements",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "themes",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    CreatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_themes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_themes_users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_achievements",
                schema: "app",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_achievements", x => new { x.UserId, x.AchievementId });
                    table.ForeignKey(
                        name: "FK_user_achievements_achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalSchema: "app",
                        principalTable: "achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_achievements_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_cards",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_cards_themes_ThemeId",
                        column: x => x.ThemeId,
                        principalSchema: "app",
                        principalTable: "themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_completed_themes",
                schema: "app",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_completed_themes", x => new { x.UserId, x.ThemeId });
                    table.ForeignKey(
                        name: "FK_user_completed_themes_themes_ThemeId",
                        column: x => x.ThemeId,
                        principalSchema: "app",
                        principalTable: "themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_completed_themes_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "app",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "correlation_quizzes",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Image1 = table.Column<string>(type: "text", nullable: false),
                    Image2 = table.Column<string>(type: "text", nullable: false),
                    Image3 = table.Column<string>(type: "text", nullable: false),
                    Text1 = table.Column<string>(type: "text", nullable: false),
                    Text2 = table.Column<string>(type: "text", nullable: false),
                    Text3 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_correlation_quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_correlation_quizzes_event_cards_EventCardId",
                        column: x => x.EventCardId,
                        principalSchema: "app",
                        principalTable: "event_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "image_quizzes",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Image1 = table.Column<string>(type: "text", nullable: false),
                    Image2 = table.Column<string>(type: "text", nullable: false),
                    Image3 = table.Column<string>(type: "text", nullable: false),
                    Image4 = table.Column<string>(type: "text", nullable: false),
                    CorrectImageIndex = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_image_quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_image_quizzes_event_cards_EventCardId",
                        column: x => x.EventCardId,
                        principalSchema: "app",
                        principalTable: "event_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "text_quizzes",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "text", nullable: false),
                    Text1 = table.Column<string>(type: "text", nullable: false),
                    Text2 = table.Column<string>(type: "text", nullable: false),
                    Text3 = table.Column<string>(type: "text", nullable: false),
                    Text4 = table.Column<string>(type: "text", nullable: false),
                    CorrectTextIndex = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_text_quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_text_quizzes_event_cards_EventCardId",
                        column: x => x.EventCardId,
                        principalSchema: "app",
                        principalTable: "event_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "true_or_false_quizzes",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    IsTrue = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_true_or_false_quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_true_or_false_quizzes_event_cards_EventCardId",
                        column: x => x.EventCardId,
                        principalSchema: "app",
                        principalTable: "event_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_achievements_Name",
                schema: "app",
                table: "achievements",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_correlation_quizzes_EventCardId",
                schema: "app",
                table: "correlation_quizzes",
                column: "EventCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_cards_ThemeId",
                schema: "app",
                table: "event_cards",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_image_quizzes_EventCardId",
                schema: "app",
                table: "image_quizzes",
                column: "EventCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_text_quizzes_EventCardId",
                schema: "app",
                table: "text_quizzes",
                column: "EventCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_themes_CreatorUserId",
                schema: "app",
                table: "themes",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_themes_CreatorUserId_Name",
                schema: "app",
                table: "themes",
                columns: new[] { "CreatorUserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_true_or_false_quizzes_EventCardId",
                schema: "app",
                table: "true_or_false_quizzes",
                column: "EventCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_achievements_AchievementId",
                schema: "app",
                table: "user_achievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_user_completed_themes_ThemeId",
                schema: "app",
                table: "user_completed_themes",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_user_completed_themes_UserId_CompletedAt",
                schema: "app",
                table: "user_completed_themes",
                columns: new[] { "UserId", "CompletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "correlation_quizzes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "image_quizzes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "text_quizzes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "true_or_false_quizzes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "user_achievements",
                schema: "app");

            migrationBuilder.DropTable(
                name: "user_completed_themes",
                schema: "app");

            migrationBuilder.DropTable(
                name: "event_cards",
                schema: "app");

            migrationBuilder.DropTable(
                name: "achievements",
                schema: "app");

            migrationBuilder.DropTable(
                name: "themes",
                schema: "app");

            migrationBuilder.DropColumn(
                name: "Score",
                schema: "app",
                table: "users");
        }
    }
}
