namespace Timecrax.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Role { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public int Score { get; set; }
    public string? SchoolName { get; set; }
    public string? Picture { get; set; }
    public int? GdprVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
    public ICollection<UserCompletedTheme> CompletedThemes { get; set; } = new List<UserCompletedTheme>();
    public ICollection<Theme> CreatedThemes { get; set; } = new List<Theme>();

}
