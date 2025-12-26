namespace Timecrax.Api.Domain.Entities;

public class TrueOrFalseQuiz
{
    public Guid Id { get; set; }

    public Guid EventCardId { get; set; }
    public EventCard EventCard { get; set; } = default!;

    public string Text { get; set; } = default!;
    public bool IsTrue { get; set; }
}
