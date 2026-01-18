namespace Timecrax.Api.Domain.Entities;

public class Medal
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
    public string Image { get; set; } = default!;
    public int MinScore { get; set; }
    public string Language { get; set; } = default!;  // PK: "en", "pt-BR", "fr", "es"


    public DateTimeOffset CreatedAt { get; set; }
}
