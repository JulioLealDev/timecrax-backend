using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Domain.Entities;

namespace Timecrax.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // ============================
    // DbSets existentes
    // ============================
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    // ============================
    // Novos DbSets
    // ============================
    public DbSet<Medal> Medals => Set<Medal>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Theme> Themes => Set<Theme>();
    public DbSet<UserCompletedTheme> UserCompletedThemes => Set<UserCompletedTheme>();
    public DbSet<EventCard> EventCards => Set<EventCard>();
    public DbSet<ImageQuiz> ImageQuizzes => Set<ImageQuiz>();
    public DbSet<TextQuiz> TextQuizzes => Set<TextQuiz>();
    public DbSet<TrueOrFalseQuiz> TrueOrFalseQuizzes => Set<TrueOrFalseQuiz>();
    public DbSet<CorrelationQuiz> CorrelationQuizzes => Set<CorrelationQuiz>();
    public DbSet<ThemeUploadSession> ThemeUploadSessions => Set<ThemeUploadSession>();
    public DbSet<ThemeUploadAsset> ThemeUploadAssets => Set<ThemeUploadAsset>();
    public DbSet<Gdpr> Gdprs => Set<Gdpr>();



    // ============================
    // Fluent API
    // ============================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Schema padrão
        modelBuilder.HasDefaultSchema("app");

        // ============================
        // USERS
        // ============================
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");

            b.HasKey(x => x.Id);

            b.HasIndex(x => x.Email)
                .IsUnique();

            b.Property(x => x.Score)
                .IsRequired()
                .HasDefaultValue(0);
        });

        // ============================
        // REFRESH TOKENS
        // ============================
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("refresh_tokens");

            b.HasKey(x => x.Id);

            b.HasIndex(x => x.TokenHash)
                .IsUnique();

            b.HasIndex(x => x.UserId);
        });

        // ============================
        // PASSWORD RESET TOKENS
        // ============================
        modelBuilder.Entity<PasswordResetToken>(b =>
        {
            b.ToTable("password_reset_tokens");

            b.HasKey(x => x.Id);

            b.Property(x => x.TokenHash)
                .IsRequired();

            b.HasIndex(x => x.TokenHash)
                .IsUnique();

            b.Property(x => x.ExpiresAt)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.UserId, x.CreatedAt });
        });

        // ============================
        // MEDALS
        // ============================
        modelBuilder.Entity<Medal>(b =>
        {
            b.ToTable("medals");

            b.HasKey(x => x.Id);

            b.Property(x => x.Name)
                .IsRequired();

            b.Property(x => x.Image)
                .IsRequired();

            b.Property(x => x.MinScore)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasIndex(x => x.Name)
                .IsUnique();

            b.HasIndex(x => x.MinScore)
                .IsUnique();
        });

        // ============================
        // ACHIEVEMENTS
        // ============================
        modelBuilder.Entity<Achievement>(b =>
        {
            b.ToTable("achievements");

            b.HasKey(x => x.Id);

            b.Property(x => x.Name)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasIndex(x => x.Name)
                .IsUnique();
        });

        // ============================
        // USER_ACHIEVEMENTS (N:N)
        // ============================
        modelBuilder.Entity<UserAchievement>(b =>
        {
            b.ToTable("user_achievements");

            b.HasKey(x => new { x.UserId, x.AchievementId });

            b.Property(x => x.AchievedAt)
                .IsRequired();

            b.HasOne(x => x.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(x => x.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.AchievementId);
        });

        // ============================
        // THEMES
        // ============================
        modelBuilder.Entity<Theme>(b =>
        {
            b.ToTable("themes");

            b.HasKey(x => x.Id);

            b.Property(x => x.Name)
                .IsRequired();

            b.Property(x => x.Image)
                .IsRequired();

            b.Property(x => x.ReadyToPlay)
                .IsRequired()
                .HasDefaultValue(false);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.Property(x => x.UpdatedAt)
                .IsRequired();

            b.HasOne(x => x.CreatorUser)
                .WithMany(u => u.CreatedThemes)
                .HasForeignKey(x => x.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.CreatorUserId);

            b.HasIndex(x => new { x.CreatorUserId, x.Name })
                .IsUnique();
        });

        // ============================
        // USER_COMPLETED_THEMES
        // ============================
        modelBuilder.Entity<UserCompletedTheme>(b =>
        {
            b.ToTable("user_completed_themes");

            b.HasKey(x => new { x.UserId, x.ThemeId });

            b.Property(x => x.CompletedAt)
                .IsRequired();

            b.HasOne(x => x.User)
                .WithMany(u => u.CompletedThemes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Theme)
                .WithMany(t => t.CompletedByUsers)
                .HasForeignKey(x => x.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.ThemeId);
            b.HasIndex(x => new { x.UserId, x.CompletedAt });
        });

        // ============================
        // EVENT_CARDS
        // ============================
        modelBuilder.Entity<EventCard>(b =>
        {
            b.ToTable("event_cards", t =>
            {
                t.HasCheckConstraint(
                    "ck_event_cards_year",
                    "\"Year\" > 0"
                );

                t.HasCheckConstraint(
                    "ck_event_cards_era",
                    "\"Era\" IN ('BC', 'AD')"
                );

                t.HasCheckConstraint(
                    "ck_event_cards_orderindex", 
                    "\"OrderIndex\" >= 0"
                );

            });

            b.HasKey(x => x.Id);

            b.Property(x => x.Image)
                .IsRequired();

            b.Property(x => x.Title)
                .IsRequired();

            b.Property(x => x.Year)
                .IsRequired();

            b.Property(x => x.Era)
                .IsRequired()
                .HasConversion<string>() // <-- MUITO IMPORTANTE
                .HasMaxLength(2);

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasOne(x => x.Theme)
                .WithMany(t => t.EventCards)
                .HasForeignKey(x => x.ThemeId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.ThemeId, x.OrderIndex }).IsUnique();

            b.HasIndex(x => new { x.ThemeId, x.CreatedAt });
        });

        // ============================
        // IMAGE_QUIZZES (1:1)
        // ============================
        modelBuilder.Entity<ImageQuiz>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Question)
                .IsRequired();

            b.Property(x => x.Image1).IsRequired();
            b.Property(x => x.Image2).IsRequired();
            b.Property(x => x.Image3).IsRequired();
            b.Property(x => x.Image4).IsRequired();

            b.Property(x => x.CorrectImageIndex)
                .IsRequired();
            b.ToTable("image_quizzes", t =>
            {
                t.HasCheckConstraint(
                    "ck_image_quizzes_correct",
                    "\"CorrectImageIndex\" BETWEEN 0 AND 3"
                );
            });

            b.HasOne(x => x.EventCard)
                .WithOne(e => e.ImageQuiz)
                .HasForeignKey<ImageQuiz>(x => x.EventCardId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.EventCardId)
                .IsUnique();
        });

        // ============================
        // TEXT_QUIZZES (1:1)
        // ============================
        modelBuilder.Entity<TextQuiz>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Question)
                .IsRequired();

            b.Property(x => x.Text1).IsRequired();
            b.Property(x => x.Text2).IsRequired();
            b.Property(x => x.Text3).IsRequired();
            b.Property(x => x.Text4).IsRequired();

            b.Property(x => x.CorrectTextIndex)
                .IsRequired();

            b.ToTable("text_quizzes", t =>
            {
                t.HasCheckConstraint(
                    "ck_text_quizzes_correct",
                    "\"CorrectTextIndex\" BETWEEN 0 AND 3"
                );
            });

            b.HasOne(x => x.EventCard)
                .WithOne(e => e.TextQuiz)
                .HasForeignKey<TextQuiz>(x => x.EventCardId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.EventCardId)
                .IsUnique();
        });

        // ============================
        // TRUE_OR_FALSE_QUIZZES (1:1)
        // ============================
        modelBuilder.Entity<TrueOrFalseQuiz>(b =>
        {
            b.ToTable("true_or_false_quizzes");

            b.HasKey(x => x.Id);

            b.Property(x => x.Text)
                .IsRequired();

            b.Property(x => x.IsTrue)
                .IsRequired();

            b.HasOne(x => x.EventCard)
                .WithOne(e => e.TrueOrFalseQuiz)
                .HasForeignKey<TrueOrFalseQuiz>(x => x.EventCardId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.EventCardId)
                .IsUnique();
        });

        // ============================
        // CORRELATION_QUIZZES (1:1)
        // ============================
        modelBuilder.Entity<CorrelationQuiz>(b =>
        {
            b.ToTable("correlation_quizzes");

            b.HasKey(x => x.Id);

            b.Property(x => x.Image1).IsRequired();
            b.Property(x => x.Image2).IsRequired();
            b.Property(x => x.Image3).IsRequired();

            b.Property(x => x.Text1).IsRequired();
            b.Property(x => x.Text2).IsRequired();
            b.Property(x => x.Text3).IsRequired();

            b.HasOne(x => x.EventCard)
                .WithOne(e => e.CorrelationQuiz)
                .HasForeignKey<CorrelationQuiz>(x => x.EventCardId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.EventCardId)
                .IsUnique();
        });

        // ============================
        // THEME UPLOAD SESSION
        // ============================
        modelBuilder.Entity<ThemeUploadSession>(b =>
        {
            b.ToTable("theme_upload_sessions");

            b.HasKey(x => x.Id);

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.LastTouchedAt).IsRequired();
            b.Property(x => x.IsClosed).IsRequired().HasDefaultValue(false);

            b.HasIndex(x => x.UserId)
                .IsUnique()
                .HasDatabaseName("UX_ThemeUploadSessions_User_OpenSession")
                .HasFilter("\"IsClosed\" = false");
            b.HasIndex(x => x.CreatedAt);

            b.HasOne(x => x.User)
                .WithMany() // sem navegação no User por enquanto
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================
        // THEME UPLOAD ASSESTS
        // ============================
        modelBuilder.Entity<ThemeUploadAsset>(b =>
        {
            b.ToTable("theme_upload_assets");

            b.HasKey(x => x.Id);

            b.Property(x => x.SlotKey)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.Url)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .IsRequired();

            b.HasOne(x => x.Session)
                .WithMany(s => s.Assets)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.SessionId, x.SlotKey })
                .IsUnique();
        });

        // ============================
        // GDPR
        // ============================
        modelBuilder.Entity<Gdpr>(b =>
        {
            b.ToTable("gdpr");

            b.HasKey(x => x.Language);

            b.Property(x => x.Language)
                .IsRequired()
                .HasMaxLength(10);

            b.Property(x => x.Version)
                .IsRequired();

            b.Property(x => x.Terms)
                .IsRequired();
        });

    }
}
