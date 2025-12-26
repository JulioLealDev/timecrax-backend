namespace Timecrax.Api.Domain.Entities;

public class Achievement
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public string? Image { get; set; }
    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
