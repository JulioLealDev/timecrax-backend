namespace Timecrax.Api.Domain.Entities;

public class Theme
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public string? Resume { get; set; }
    public string Image { get; set; } = default!;
    public bool ReadyToPlay { get; set; }

    public Guid CreatorUserId { get; set; }
    public User CreatorUser { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<EventCard> EventCards { get; set; } = new List<EventCard>();
    public ICollection<UserCompletedTheme> CompletedByUsers { get; set; } = new List<UserCompletedTheme>();
}
