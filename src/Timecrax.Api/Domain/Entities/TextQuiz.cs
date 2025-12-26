namespace Timecrax.Api.Domain.Entities;

public class TextQuiz
{
    public Guid Id { get; set; }

    public Guid EventCardId { get; set; }
    public EventCard EventCard { get; set; } = default!;

    public string Question { get; set; } = default!;

    public string Text1 { get; set; } = default!;
    public string Text2 { get; set; } = default!;
    public string Text3 { get; set; } = default!;
    public string Text4 { get; set; } = default!;

    public short CorrectTextIndex { get; set; }
}
