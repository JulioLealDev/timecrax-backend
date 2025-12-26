using Microsoft.AspNetCore.Http;

namespace Timecrax.Api.Dtos.Theme;

public class CreateThemeFormRequest
{
    public string Name { get; set; } = default!;

    // Arquivo da imagem do tema (só vem aqui, no create theme)
    public IFormFile? Image { get; set; }

    // JSON string com as cartas (e demais campos)
    public string CardsJson { get; set; } = default!;
}
