namespace Timecrax.Api.Domain.Entities;
using Timecrax.Api.Domain.Enums;


public class EventCard
{
    public Guid Id { get; set; }

    public Guid ThemeId { get; set; }
    public Theme Theme { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Image { get; set; } = default!;
    public int Year { get; set; }
    public Era Era { get; set; }
    public int OrderIndex { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ImageQuiz ImageQuiz { get; set; } = default!;
    public TextQuiz TextQuiz { get; set; } = default!;
    public TrueOrFalseQuiz TrueOrFalseQuiz { get; set; } = default!;
    public CorrelationQuiz CorrelationQuiz { get; set; } = default!;
}
