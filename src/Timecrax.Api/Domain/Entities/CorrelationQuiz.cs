namespace Timecrax.Api.Domain.Entities;

public class CorrelationQuiz
{
    public Guid Id { get; set; }

    public Guid EventCardId { get; set; }
    public EventCard EventCard { get; set; } = default!;

    public string Image1 { get; set; } = default!;
    public string Image2 { get; set; } = default!;
    public string Image3 { get; set; } = default!;

    public string Text1 { get; set; } = default!;
    public string Text2 { get; set; } = default!;
    public string Text3 { get; set; } = default!;
}
