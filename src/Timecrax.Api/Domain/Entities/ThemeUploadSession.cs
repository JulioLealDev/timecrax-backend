namespace Timecrax.Api.Domain.Entities;

public class ThemeUploadSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastTouchedAt { get; set; } // útil p/ expirar

    public bool IsClosed { get; set; }        // opcional: fecha após criar theme

    public ICollection<ThemeUploadAsset> Assets { get; set; } = new List<ThemeUploadAsset>();

}
