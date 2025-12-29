namespace Timecrax.Api.Domain.Entities;

public class Gdpr
{
    public string Language { get; set; } = default!;  // PK: "en", "pt-BR", "fr", "es"
    public int Version { get; set; }
    public string Terms { get; set; } = default!;
}
