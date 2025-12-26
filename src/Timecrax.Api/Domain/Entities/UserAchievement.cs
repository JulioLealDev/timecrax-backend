namespace Timecrax.Api.Domain.Entities;

public class UserAchievement
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid AchievementId { get; set; }
    public Achievement Achievement { get; set; } = default!;

    public DateTimeOffset AchievedAt { get; set; }
}
