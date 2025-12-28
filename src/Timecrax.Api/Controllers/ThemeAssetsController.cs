using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Timecrax.Api.Data;
using Timecrax.Api.Domain.Entities;
using Timecrax.Api.Dtos.ThemeAssets;
using Timecrax.Api.Extensions;

namespace Timecrax.Api.Controllers;

[ApiController]
[Route("theme-assets")]
[Authorize(Roles = "teacher")]
public class ThemeAssetsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public ThemeAssetsController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("sessions/{sessionId:guid}/upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> UploadAsset(
        [FromRoute] Guid sessionId,
        [FromForm] ThemeAssetUploadRequest req,
        CancellationToken ct)
    {
        var file = req.File;
        var slotKey = (req.SlotKey ?? "").Trim();

        if (file is null || file.Length == 0)
            return BadRequest("Arquivo inválido.");

        if (string.IsNullOrWhiteSpace(slotKey))
            return BadRequest("slotKey é obrigatório.");

        if (slotKey.Length > 200)
            return BadRequest("slotKey excede o tamanho máximo (200).");

        var ok =
            Regex.IsMatch(slotKey, @"^cards\[\d+\]\.imageUrl$", RegexOptions.CultureInvariant) ||
            Regex.IsMatch(slotKey, @"^cards\[\d+\]\.imageQuiz\.options\[\d+\]\.imageUrl$", RegexOptions.CultureInvariant) ||
            Regex.IsMatch(slotKey, @"^cards\[\d+\]\.correlationQuiz\.items\[\d+\]\.imageUrl$", RegexOptions.CultureInvariant);

        if (!ok)
            return BadRequest("slotKey inválido. Use: cards[i].imageUrl | cards[i].imageQuiz.options[k].imageUrl | cards[i].correlationQuiz.items[k].imageUrl");

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Apenas imagens são permitidas.");

        var root = (_config["Storage:RootPath"] ?? "").Trim();
        var publicBase = (_config["Storage:PublicBasePath"] ?? "").TrimEnd('/');

        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(publicBase))
            return StatusCode(500, "Storage não configurado (Storage:RootPath / Storage:PublicBasePath).");

        var userId = User.GetUserId();

        var session = await _db.ThemeUploadSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsClosed, ct);

        if (session is null)
            return NotFound("Sessão inválida ou encerrada.");

        session.LastTouchedAt = DateTimeOffset.UtcNow;

        // Se sessão tem ThemeId (modo edit), usa ele. Caso contrário usa SessionId (modo create/staging)
        var themeId = session.ThemeId ?? sessionId;

        // monta o caminho final determinístico (arquivo .webp)
        var relative = GetRelativePathForSlotKey(slotKey); // ex: cards/0/main.webp
        var themeDir = Path.Combine(root, "themes", themeId.ToString());
        var fullPath = Path.Combine(themeDir, relative);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // carrega e converte -> webp (padronizado)
        Image img;
        try
        {
            img = await Image.LoadAsync(file.OpenReadStream(), ct);
        }
        catch
        {
            return BadRequest("Imagem inválida ou formato não suportado.");
        }

        using (img)
        {
            // Otimização agressiva: redimensiona para tamanhos menores e qualidade menor
            int maxDimension = 1200; // Reduzido de 1920 para 1200
            if (img.Width > maxDimension || img.Height > maxDimension)
            {
                var ratio = Math.Min((double)maxDimension / img.Width, (double)maxDimension / img.Height);
                var newWidth = (int)(img.Width * ratio);
                var newHeight = (int)(img.Height * ratio);
                img.Mutate(x => x.Resize(newWidth, newHeight, KnownResamplers.Box)); // Box é mais rápido
            }

            await using (var fs = System.IO.File.Create(fullPath))
            {
                // Qualidade reduzida para 50 e método mais rápido
                await img.SaveAsWebpAsync(fs, new WebpEncoder
                {
                    Quality = 50,
                    Method = WebpEncodingMethod.Fastest // Prioriza velocidade
                }, ct);
            }
        }

        var url = $"{publicBase}/themes/{themeId}/{relative.Replace('\\', '/')}";

        // upsert por (SessionId, SlotKey)
        var asset = await _db.ThemeUploadAssets
            .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.SlotKey == slotKey, ct);

        if (asset is null)
        {
            asset = new ThemeUploadAsset
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                SlotKey = slotKey,
                Url = url,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _db.ThemeUploadAssets.Add(asset);
        }
        else
        {
            // remove arquivo anterior (se URL mudou)
            TryDeleteOldAssetFile(asset.Url, publicBase, root);
            asset.Url = url;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new { slotKey, url });
    }

    private static void TryDeleteOldAssetFile(string oldUrl, string publicBase, string root)
    {
        if (string.IsNullOrWhiteSpace(oldUrl)) return;
        if (!oldUrl.StartsWith(publicBase, StringComparison.OrdinalIgnoreCase)) return;

        // oldUrl = {publicBase}/themes/{themeId}/cards/...
        var relative = oldUrl.Substring(publicBase.Length).TrimStart('/'); // themes/...
        var physical = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));

        var fullRoot = Path.GetFullPath(root);
        var fullPath = Path.GetFullPath(physical);

        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            return;

        if (System.IO.File.Exists(physical))
            System.IO.File.Delete(physical);
    }

    private static string GetRelativePathForSlotKey(string slotKey)
    {
        // cards[i].imageUrl -> cards/i/main.webp
        var m1 = Regex.Match(slotKey, @"^cards\[(\d+)\]\.imageUrl$");
        if (m1.Success)
        {
            var i = m1.Groups[1].Value;
            return Path.Combine("cards", i, "main.webp");
        }

        // cards[i].imageQuiz.options[k].imageUrl -> cards/i/imageQuiz/options/k.webp
        var m2 = Regex.Match(slotKey, @"^cards\[(\d+)\]\.imageQuiz\.options\[(\d+)\]\.imageUrl$");
        if (m2.Success)
        {
            var i = m2.Groups[1].Value;
            var k = m2.Groups[2].Value;
            return Path.Combine("cards", i, "imageQuiz", "options", $"{k}.webp");
        }

        // cards[i].correlationQuiz.items[k].imageUrl -> cards/i/correlation/items/k.webp
        var m3 = Regex.Match(slotKey, @"^cards\[(\d+)\]\.correlationQuiz\.items\[(\d+)\]\.imageUrl$");
        if (m3.Success)
        {
            var i = m3.Groups[1].Value;
            var k = m3.Groups[2].Value;
            return Path.Combine("cards", i, "correlation", "items", $"{k}.webp");
        }

        throw new InvalidOperationException("slotKey inválido para path.");
    }
    
    /// <summary>
    /// Deleta todos os assets (imagens) de uma carta específica.
    /// Remove registros do banco e arquivos físicos do storage.
    /// </summary>
    [HttpDelete("sessions/{sessionId:guid}/cards/{cardIndex:int}")]
    public async Task<IActionResult> DeleteCardAssets(
        [FromRoute] Guid sessionId,
        [FromRoute] int cardIndex,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        var session = await _db.ThemeUploadSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsClosed, ct);

        if (session is null)
            return NotFound("Sessão inválida ou encerrada.");

        var root = (_config["Storage:RootPath"] ?? "").Trim();
        var publicBase = (_config["Storage:PublicBasePath"] ?? "").TrimEnd('/');

        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(publicBase))
            return StatusCode(500, "Storage não configurado.");

        // Padrões de slotKey para a carta (8 imagens no total):
        // - cards[i].imageUrl (1)
        // - cards[i].imageQuiz.options[0-3].imageUrl (4)
        // - cards[i].correlationQuiz.items[0-2].imageUrl (3)
        var slotKeyPatterns = new List<string>
        {
            $"cards[{cardIndex}].imageUrl"
        };

        for (int k = 0; k < 4; k++)
            slotKeyPatterns.Add($"cards[{cardIndex}].imageQuiz.options[{k}].imageUrl");

        for (int k = 0; k < 3; k++)
            slotKeyPatterns.Add($"cards[{cardIndex}].correlationQuiz.items[{k}].imageUrl");

        // Busca todos os assets relacionados à carta
        var assets = await _db.ThemeUploadAssets
            .Where(a => a.SessionId == sessionId && slotKeyPatterns.Contains(a.SlotKey))
            .ToListAsync(ct);

        // Deleta arquivos físicos
        foreach (var asset in assets)
        {
            TryDeleteOldAssetFile(asset.Url, publicBase, root);
        }

        // Deleta a pasta da carta se existir
        var themeId = session.ThemeId ?? session.Id;
        var cardFolderPath = Path.Combine(root, "themes", themeId.ToString(), "cards", cardIndex.ToString());
        if (Directory.Exists(cardFolderPath))
        {
            try
            {
                Directory.Delete(cardFolderPath, recursive: true);
            }
            catch (Exception ex)
            {
                // Log error but continue - não é crítico
                Console.WriteLine($"Warning: Failed to delete card folder {cardIndex}: {ex.Message}");
            }
        }

        // Remove registros do banco
        _db.ThemeUploadAssets.RemoveRange(assets);

        session.LastTouchedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { deletedCount = assets.Count, message = $"Carta {cardIndex} e seus {assets.Count} assets deletados com sucesso." });
    }

    /// <summary>
    /// Cria uma sessão de upload de assets (imagens) para criação ou edição de um Theme.
    /// O front chama isso ao entrar em /create-theme ou ao editar um tema existente.
    /// Garante no máximo UMA sessão aberta por usuário.
    /// </summary>
    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest? req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var now = DateTimeOffset.UtcNow;
        Guid? themeId = req?.ThemeId;

        // Se themeId fornecido, valida que o tema existe e pertence ao usuário
        if (themeId.HasValue)
        {
            var themeExists = await _db.Themes
                .AnyAsync(t => t.Id == themeId.Value && t.CreatorUserId == userId, ct);

            if (!themeExists)
                return BadRequest("Tema não encontrado ou você não tem permissão para editá-lo.");
        }

        // Fecha sessões antigas abertas do mesmo usuário (política de limpeza)
        var openSessions = await _db.ThemeUploadSessions
            .Where(s => s.UserId == userId && !s.IsClosed)
            .ToListAsync(ct);

        foreach (var s in openSessions)
        {
            s.IsClosed = true;
            s.LastTouchedAt = now;
        }

        // Cria nova sessão
        var session = new ThemeUploadSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ThemeId = themeId,
            CreatedAt = now,
            LastTouchedAt = now,
            IsClosed = false
        };

        _db.ThemeUploadSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            sessionId = session.Id,
            themeId = session.ThemeId,
            createdAt = session.CreatedAt
        });
    }

    public class CreateSessionRequest
    {
        public Guid? ThemeId { get; set; }
    }

}
