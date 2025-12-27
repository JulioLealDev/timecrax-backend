namespace Timecrax.Api.Domain.Entities;

public class Rank
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public string Image { get; set; } = default!;
    public int MinScore { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
