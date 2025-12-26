using Timecrax.Api.Dtos.Theme;
using Timecrax.Api.Domain.Exceptions;

namespace Timecrax.Api.Domain.Validators;

public static class ThemeValidator
{
    public static Dictionary<string, string> ValidateForCreate(ThemeDto dto)
    {
        var e = ValidateCommon(dto);

        // Create: capa deve ser dataUrl (você disse: será a única base64)
        if (string.IsNullOrWhiteSpace(dto.Image))
            e["theme.image"] = "Imagem do tema é obrigatória.";
        else if (!IsDataUrl(dto.Image))
            e["theme.image"] = "Na criação, a imagem do tema deve ser dataUrl (base64).";

        // Create: sessão é obrigatória
        if (dto.UploadSessionId is null || dto.UploadSessionId == Guid.Empty)
            e["theme.uploadSessionId"] = "UploadSessionId é obrigatório.";

        return e;
    }

    public static Dictionary<string, string> ValidateForUpdate(ThemeDto dto)
    {
        var e = ValidateCommon(dto);

        // Update: permite URL ou dataUrl (se você aceitar trocar capa via base64)
        if (string.IsNullOrWhiteSpace(dto.Image))
            e["theme.image"] = "Imagem do tema é obrigatória.";
        else if (!IsDataUrl(dto.Image) && !IsHttpUrl(dto.Image))
            e["theme.image"] = "Imagem do tema deve ser dataUrl (base64) ou uma URL válida.";

        // Update: NÃO exigir sessão
        // dto.UploadSessionId pode ser null
        return e;
    }

    public static Dictionary<string, string> ValidateCommon(ThemeDto dto)
    {
        var e = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(dto.Name))
            e["theme.name"] = "Nome do tema é obrigatório.";

        if (string.IsNullOrWhiteSpace(dto.Image))
            e["theme.image"] = "Imagem do tema é obrigatória.";
        else
        {
            // permite dataUrl OU URL(no caso de edição)
            if (!IsDataUrl(dto.Image) && !IsHttpUrl(dto.Image))
                e["theme.image"] = "Imagem do tema deve ser dataUrl (base64) ou uma URL válida.";
         }

        //if (dto.Cards is null || dto.Cards.Count < 12)
        //    e["theme.cards"] = "São necessárias pelo menos 12 cartas para criar baralho.";

        if (dto.Cards is null) return e;
        var duplicates = dto.Cards
            .GroupBy(x => x.OrderIndex)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var d in duplicates)
            e[$"cards.orderIndex[{d}]"] = "OrderIndex duplicado.";

        for (var i = 0; i < dto.Cards.Count; i++)
        {
            var c = dto.Cards[i];

            if (c.Year <= 0)
                e[$"cards[{i}].year"] = "Ano deve ser maior que 0.";

            if (string.IsNullOrWhiteSpace(c.Era))
                e[$"cards[{i}].era"] = "Era é obrigatória.";

            else if (c.Era != "AC" && c.Era != "DC")
                e[$"cards[{i}].era"] = "Era inválida. Use AC ou DC.";

            if (string.IsNullOrWhiteSpace(c.Caption))
                e[$"cards[{i}].caption"] = "Texto (caption) é obrigatório.";

            ValidateUrlNotBase64(e, $"cards[{i}].imageUrl", c.ImageUrl);

            if (c.OrderIndex < 0)
                e[$"cards[{i}].orderIndex"] = "OrderIndex não pode ser negativo.";

            // ImageQuiz
            if (c.ImageQuiz is null) e[$"cards[{i}].imageQuiz"] = "ImageQuiz é obrigatório.";
            else
            {
                if (string.IsNullOrWhiteSpace(c.ImageQuiz.Question))
                    e[$"cards[{i}].imageQuiz.question"] = "Pergunta do ImageQuiz é obrigatória.";

                if (c.ImageQuiz.Options is null || c.ImageQuiz.Options.Count != 4)
                    e[$"cards[{i}].imageQuiz.options"] = "ImageQuiz precisa de 4 opções.";
                else
                {
                    for (var j = 0; j < c.ImageQuiz.Options.Count; j++)
                        ValidateUrlNotBase64(e, $"cards[{i}].imageQuiz.options[{j}].imageUrl", c.ImageQuiz.Options[j].ImageUrl);
                }

                if (c.ImageQuiz.CorrectIndex < 0 || c.ImageQuiz.CorrectIndex > 3)
                    e[$"cards[{i}].imageQuiz.correctIndex"] = "CorrectIndex inválido (0..3).";
            }

            // TextQuiz
            if (c.TextQuiz is null) e[$"cards[{i}].textQuiz"] = "TextQuiz é obrigatório.";
            else
            {
                if (string.IsNullOrWhiteSpace(c.TextQuiz.Question))
                    e[$"cards[{i}].textQuiz.question"] = "Pergunta do TextQuiz é obrigatória.";

                if (c.TextQuiz.Options is null || c.TextQuiz.Options.Count != 4)
                    e[$"cards[{i}].textQuiz.options"] = "TextQuiz precisa de 4 opções.";
                else
                {
                    for (var j = 0; j < c.TextQuiz.Options.Count; j++)
                        if (string.IsNullOrWhiteSpace(c.TextQuiz.Options[j].Text))
                            e[$"cards[{i}].textQuiz.options[{j}].text"] = "Texto da opção é obrigatório.";
                }

                if (c.TextQuiz.CorrectIndex < 0 || c.TextQuiz.CorrectIndex > 3)
                    e[$"cards[{i}].textQuiz.correctIndex"] = "CorrectIndex inválido (0..3).";
            }

            // TrueFalse
            if (c.TrueFalseQuiz is null) e[$"cards[{i}].trueFalseQuiz"] = "TrueFalseQuiz é obrigatório.";
            else if (string.IsNullOrWhiteSpace(c.TrueFalseQuiz.Statement))
                e[$"cards[{i}].trueFalseQuiz.statement"] = "Statement é obrigatório.";

            // Correlation
            if (c.CorrelationQuiz is null) e[$"cards[{i}].correlationQuiz"] = "CorrelationQuiz é obrigatório.";
            else
            {

                if (c.CorrelationQuiz.Items is null || c.CorrelationQuiz.Items.Count != 3)
                    e[$"cards[{i}].correlationQuiz.items"] = "CorrelationQuiz precisa de 3 itens.";
                else
                {
                    for (var j = 0; j < c.CorrelationQuiz.Items.Count; j++)
                    {
                        if (string.IsNullOrWhiteSpace(c.CorrelationQuiz.Items[j].Text))
                            e[$"cards[{i}].correlationQuiz.items[{j}].text"] = "Texto é obrigatório.";

                        ValidateUrlNotBase64(e, $"cards[{i}].correlationQuiz.items[{j}].imageUrl", c.CorrelationQuiz.Items[j].ImageUrl);
                    }
                }
            }
        }

        return e;
    }
    
    private static void ValidateUrlNotBase64(Dictionary<string, string> e, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            e[key] = "Imagem é obrigatória.";
            return;
        }

        if (IsDataUrl(value))
        {
            e[key] = "Imagem deve ser URL (não base64).";
            return;
        }

        //if (!IsHttpUrl(value))
       // {
        //    e[key] = "Imagem deve ser uma URL válida.";
       // }
    }

    private static bool IsDataUrl(string? s)
        => !string.IsNullOrWhiteSpace(s) && s.TrimStart().StartsWith("data:image/", StringComparison.OrdinalIgnoreCase);

    private static bool IsHttpUrl(string? s)
        => Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

}
