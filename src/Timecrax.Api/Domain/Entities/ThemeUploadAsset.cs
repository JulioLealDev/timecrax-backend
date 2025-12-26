namespace Timecrax.Api.Domain.Entities;

public class ThemeUploadAsset
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }
    public ThemeUploadSession Session { get; set; } = default!;

    // Ex: "cards[0].imageQuiz.option[2]"
    public string SlotKey { get; set; } = default!;

    // URL pública final
    public string Url { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}
