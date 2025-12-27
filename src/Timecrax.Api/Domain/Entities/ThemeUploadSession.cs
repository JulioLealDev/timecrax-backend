namespace Timecrax.Api.Domain.Entities;

public class ThemeUploadSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    // Se não-nulo, esta sessão é para EDITAR um tema existente (uploads vão direto para themes/{ThemeId}/)
    // Se nulo, esta sessão é para CRIAR novo tema (uploads vão para staging: themes/{SessionId}/)
    public Guid? ThemeId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastTouchedAt { get; set; } // útil p/ expirar

    public bool IsClosed { get; set; }        // opcional: fecha após criar theme

    public ICollection<ThemeUploadAsset> Assets { get; set; } = new List<ThemeUploadAsset>();

}
