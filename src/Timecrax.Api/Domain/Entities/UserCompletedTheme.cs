namespace Timecrax.Api.Domain.Entities;

public class UserCompletedTheme
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid ThemeId { get; set; }
    public Theme Theme { get; set; } = default!;

    public DateTimeOffset CompletedAt { get; set; }
}
