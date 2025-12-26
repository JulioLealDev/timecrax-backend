namespace Timecrax.Api.Domain.Entities;

public class ImageQuiz
{
    public Guid Id { get; set; }

    public Guid EventCardId { get; set; }
    public EventCard EventCard { get; set; } = default!;

    public string Question { get; set; } = default!;

    public string Image1 { get; set; } = default!;
    public string Image2 { get; set; } = default!;
    public string Image3 { get; set; } = default!;
    public string Image4 { get; set; } = default!;

    public short CorrectImageIndex { get; set; }
}
