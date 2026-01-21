using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Timecrax.Api.Data;
using Timecrax.Api.Domain.Mappers;
using Timecrax.Api.Dtos.Theme;
using Timecrax.Api.Domain.Validators;
using Timecrax.Api.Extensions;
using SixLabors.ImageSharp;
using Timecrax.Api.Services;
using Timecrax.Api.Domain.Uploads;
using Timecrax.Api.Domain.Entities;

namespace Timecrax.Api.Controllers;

[ApiController]
[Route("themes")]
public class ThemesController : ControllerBase
{
    // POST /themes
    [HttpPost]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> Create(
        [FromBody] ThemeDto dto,
        [FromServices] AppDbContext db,
        [FromServices] IConfiguration config,
        [FromServices] StorageImageService storage,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        var errors = ThemeValidator.ValidateForCreate(dto);
        if (errors.Count > 0) return BadRequest(new { errors });

        var userExists = await db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists) return Unauthorized();

        if (dto.UploadSessionId is null || dto.UploadSessionId == Guid.Empty)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string>
                {
                    ["theme.uploadSessionId"] = "UploadSessionId é obrigatório para criar um tema."
                }
            });
        }

        var sessionId = dto.UploadSessionId.Value;

        // 1) validar sessão
        var session = await db.ThemeUploadSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && !s.IsClosed, ct);

        if (session is null)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string>
                {
                    ["theme.uploadSessionId"] = "Sessão inválida, encerrada ou não pertence ao usuário."
                }
            });
        }

        // 2) validar URLs do payload vs sessão (SOMENTE links, não a capa base64)
        var publicBase = (config["Storage:PublicBasePath"] ?? "").TrimEnd('/');
        if (string.IsNullOrWhiteSpace(publicBase))
            return StatusCode(500, "Storage:PublicBasePath não configurado.");

        var dbAssets = await db.ThemeUploadAssets
            .Where(a => a.SessionId == sessionId)
            .ToDictionaryAsync(a => a.SlotKey, a => a.Url, ct);

        var expected = BuildExpectedSlotsForLinksOnly(dto);

        var slotErrors = new Dictionary<string, string>();

        foreach (var (slotKey, url) in expected)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                slotErrors[slotKey] = "URL da imagem não informada.";
                continue;
            }

            var u = NormalizePublicUrl(url, publicBase);

            /*// regra 1: pertence ao storage público
            if (string.IsNullOrWhiteSpace(u) ||
                !u.StartsWith(publicBase + "/", StringComparison.OrdinalIgnoreCase))
            {
                slotErrors[slotKey] = "URL não pertence ao storage do servidor.";
                continue;
            }


            // regra 2: pertence ao diretório desta sessão
            var expectedPrefix = $"{publicBase}/themes/{sessionId}/";
            if (!u.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                slotErrors[slotKey] = "URL não pertence à sessão de upload informada.";
                continue;
            }*/

            // regra 3: existe no banco para aquele slot e bate a URL
            if (!dbAssets.TryGetValue(slotKey, out var dbUrl))
            {
                slotErrors[slotKey] = "SlotKey não encontrado na sessão.";
                continue;
            }

            var dbNorm = NormalizePublicUrl(dbUrl, publicBase);
            if (!string.Equals(dbNorm, u, StringComparison.OrdinalIgnoreCase))
            {
                slotErrors[slotKey] = "URL não corresponde ao arquivo enviado para este slot.";
                continue;
            }

        }

        if (slotErrors.Count > 0)
            return BadRequest(new { errors = slotErrors });

        // 3) criar theme com a URL da capa já pronta
        var themeId = sessionId;

        var themeAlreadyExists = await db.Themes.AnyAsync(t => t.Id == themeId, ct);
        if (themeAlreadyExists)
        {
            return Conflict(new
            {
                errors = new Dictionary<string, string>
                {
                    ["theme.id"] = "Já existe um tema criado para este UploadSessionId."
                }
            });
        }

        // 4) salvar a capa do tema (base64) no storage e substituir por URL
        string coverUrl;
        try
        {
            coverUrl = await storage.SaveThemeCoverFromDataUrlAsync(themeId, dto.Image!, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                errors = new Dictionary<string, string>
                {
                    ["theme.image"] = ex.Message
                }
            });
        }

        var dtoForPersist = dto with { Image = coverUrl }; // record -> cria cópia
        var theme = ThemeMapper.ToEntity(dtoForPersist, userId, themeId);

        db.Themes.Add(theme);

        // 5) fechar sessão
        session.IsClosed = true;
        session.LastTouchedAt = DateTimeOffset.UtcNow;

        // 6) apagar registros temporários de upload assets desta sessão
        await db.ThemeUploadAssets
            .Where(a => a.SessionId == sessionId)
            .ExecuteDeleteAsync(ct);

        await db.SaveChangesAsync(ct);

        return Created($"/themes/{theme.Id}", new { id = theme.Id });
    }


    // ==========================================================
    // Helpers (coloque dentro do ThemesController)
    // ==========================================================

    // slotKey => url (somente links; capa não entra)
    private static Dictionary<string, string> BuildExpectedSlotsForLinksOnly(ThemeDto dto)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (dto.Cards is null) return map;

        foreach (var c in dto.Cards)
        {
            var i = c.OrderIndex;

            map[ThemeUploadSlots.CardImageUrl(i)] = c.ImageUrl;

            // evita NullReference se payload vier quebrado
            var imgOpts = c.ImageQuiz?.Options ?? new List<ImageOptionDto>();
            for (var k = 0; k < imgOpts.Count; k++)
                map[ThemeUploadSlots.ImageQuizOptionImageUrl(i, k)] = imgOpts[k].ImageUrl;

            var corrItems = c.CorrelationQuiz?.Items ?? new List<CorrelationItemDto>();
            for (var k = 0; k < corrItems.Count; k++)
                map[ThemeUploadSlots.CorrelationItemImageUrl(i, k)] = corrItems[k].ImageUrl;
        }

        return map;
    }


    // PUT /themes/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] ThemeDto dto,
        [FromServices] AppDbContext db,
        [FromServices] IConfiguration config,
        [FromServices] StorageImageService storage,
        CancellationToken ct)
    {
        try
        {
            var userId = User.GetUserId();

            var errors = ThemeValidator.ValidateForUpdate(dto);
            if (errors.Count > 0) return BadRequest(new { errors });

        var theme = await db.Themes
            .Include(t => t.EventCards).ThenInclude(c => c.ImageQuiz)
            .Include(t => t.EventCards).ThenInclude(c => c.TextQuiz)
            .Include(t => t.EventCards).ThenInclude(c => c.TrueOrFalseQuiz)
            .Include(t => t.EventCards).ThenInclude(c => c.CorrelationQuiz)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (theme is null) return NotFound();
        if (theme.CreatorUserId != userId) return Forbid();

        var root = (config["Storage:RootPath"] ?? "").Trim();
        var publicBase = (config["Storage:PublicBasePath"] ?? "").TrimEnd('/');

        if (string.IsNullOrWhiteSpace(root))
            return StatusCode(500, "Storage:RootPath não configurado.");

        if (string.IsNullOrWhiteSpace(publicBase))
            return StatusCode(500, "Storage:PublicBasePath não configurado.");

        // 1) capa do tema: se vier dataUrl, salva e substitui por URL
        if (!string.IsNullOrWhiteSpace(dto.Image) &&
            dto.Image.TrimStart().StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var coverUrl = await storage.SaveThemeCoverFromDataUrlAsync(id, dto.Image, ct);
                dto = dto with { Image = coverUrl };
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string> { ["theme.image"] = ex.Message }
                });
            }
        }

        // 2) Opção A: staging na edição
        Guid? uploadSessionId =
            dto.UploadSessionId.HasValue && dto.UploadSessionId.Value != Guid.Empty
                ? dto.UploadSessionId.Value
                : (Guid?)null;

        ThemeUploadSession? uploadSession = null;
        Dictionary<string, string> sessionAssets = new(StringComparer.OrdinalIgnoreCase);

        if (uploadSessionId.HasValue)
        {
            // Proteção importante: não aceitar "sessão == tema"
            if (uploadSessionId.Value == id)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string>
                    {
                        ["theme.uploadSessionId"] = "UploadSessionId não pode ser igual ao id do tema durante edição."
                    }
                });
            }

            uploadSession = await db.ThemeUploadSessions
                .FirstOrDefaultAsync(s =>
                    s.Id == uploadSessionId.Value &&
                    s.UserId == userId &&
                    !s.IsClosed, ct);

            if (uploadSession is null)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string>
                    {
                        ["theme.uploadSessionId"] = "Sessão inválida, encerrada ou não pertence ao usuário."
                    }
                });
            }

            // puxa assets da sessão para validar "slotKey -> url"
            sessionAssets = await db.ThemeUploadAssets
                .Where(a => a.SessionId == uploadSessionId.Value)
                .ToDictionaryAsync(a => a.SlotKey, a => a.Url, ct);
        }

        // 3) valida URLs (podem ser do tema OU da sessão)
        var (slotErrors, slotsFromSession) = ValidateLinksForUpdate(dto, id, uploadSessionId, publicBase, sessionAssets);
        if (slotErrors.Count > 0)
            return BadRequest(new { errors = slotErrors });

        // 4) promover arquivos staging -> final, reescrever DTO e fechar sessão
        if (uploadSessionId.HasValue)
        {
            try
            {
                // Se sessão tem ThemeId == tema atual, os assets já estão no lugar certo (não precisa promover)
                // Apenas promove se sessão for staging (ThemeId == null)
                bool needsPromotion = uploadSession!.ThemeId == null || uploadSession.ThemeId != id;

                if (needsPromotion && slotsFromSession.Count > 0)
                    dto = RewriteDtoUrlsPromoted(dto, uploadSessionId.Value, id, publicBase, root, slotsFromSession);

                // recomendação: sempre fechar sessão ao concluir o PUT (mesmo se slotsFromSession = 0)
                uploadSession!.IsClosed = true;
                uploadSession.LastTouchedAt = DateTimeOffset.UtcNow;
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    errors = new Dictionary<string, string>
                    {
                        ["theme.uploadSessionId"] = ex.Message
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Falha ao promover assets da sessão: {ex.Message}");
            }
        }

            // 5) persistência
            theme.Name = dto.Name.Trim();
            theme.Image = dto.Image!.Trim();
            theme.UpdatedAt = DateTimeOffset.UtcNow;

            // Remove cartas antigas usando query SQL direta (mais seguro para concorrência)
            var oldCardIds = theme.EventCards.Select(c => c.Id).ToList();

            if (oldCardIds.Any())
            {
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM app.event_cards WHERE \"ThemeId\" = {id}", ct);
            }

            // Limpa o ChangeTracker para evitar conflitos
            db.ChangeTracker.Clear();

            // Recarrega o tema
            theme = await db.Themes.FirstOrDefaultAsync(t => t.Id == id, ct);
            if (theme is null) return NotFound();

            var newCards = ThemeMapper.ToCards(dto.Cards, theme.Id);

            // Adiciona novas cartas
            db.EventCards.AddRange(newCards);

            theme.Name = dto.Name.Trim();
            theme.Resume = dto.Resume?.Trim();

            // Normaliza URL da imagem para garantir que tenha baseUrl completo
            var themeImageUrl = dto.Image!.Trim();
            if (!themeImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !themeImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = (config["App:BaseUrl"] ?? "http://localhost:5139").TrimEnd('/');
                themeImageUrl = themeImageUrl.StartsWith("/")
                    ? $"{baseUrl}{themeImageUrl}"
                    : $"{baseUrl}/{themeImageUrl}";
            }
            theme.Image = themeImageUrl;

            theme.UpdatedAt = DateTimeOffset.UtcNow;
            theme.ReadyToPlay = newCards.Count >= 12;

            await db.SaveChangesAsync(ct);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update theme", details = ex.Message });
        }
    }

    private static (Dictionary<string, string> errors, HashSet<string> slotsFromSession)
    ValidateLinksForUpdate(
        ThemeDto dto,
        Guid themeId,
        Guid? uploadSessionId,
        string publicBase,
        Dictionary<string, string> sessionAssets)
    {
        var expected = BuildExpectedSlotsForLinksOnly(dto);
        var errors = new Dictionary<string, string>();
        var slotsFromSession = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var themePrefix = $"{publicBase}/themes/{themeId}/";
        var sessionPrefix = uploadSessionId.HasValue ? $"{publicBase}/themes/{uploadSessionId.Value}/" : null;

        foreach (var (slotKey, url) in expected)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                errors[slotKey] = "URL da imagem não informada.";
                continue;
            }

            var u = NormalizePublicUrl(url, publicBase);

            /*if (string.IsNullOrWhiteSpace(u) ||
                !u.StartsWith(publicBase + "/", StringComparison.OrdinalIgnoreCase))
            {
                errors[slotKey] = "URL não pertence ao storage do servidor.";
                continue;
            }*/


            // Já pertence ao tema (ok)
            if (u.StartsWith(themePrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            // Pertence à sessão atual (staging)
            if (sessionPrefix is not null && u.StartsWith(sessionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!sessionAssets.TryGetValue(slotKey, out var dbUrl))
                {
                    errors[slotKey] = "SlotKey não encontrado na sessão de upload.";
                    continue;
                }

                var dbNorm = NormalizePublicUrl(dbUrl, publicBase);

                if (!string.Equals(dbNorm, u, StringComparison.OrdinalIgnoreCase))
                {
                    errors[slotKey] = "URL não corresponde ao arquivo enviado para este slot na sessão.";
                    continue;
                }


                slotsFromSession.Add(slotKey);
                continue;
            }

            errors[slotKey] = "URL não pertence ao tema nem à sessão de upload atual.";
        }

        return (errors, slotsFromSession);
    }

    private static string PromoteOneAssetFromSessionToTheme(
        string sourceUrl,
        Guid uploadSessionId,
        Guid themeId,
        string publicBase,
        string root)
    {
        if (uploadSessionId == themeId)
            return sourceUrl; // proteção extra

        // sourceUrl já foi validada para começar com publicBase
        var fromRelative = sourceUrl.Substring(publicBase.Length).TrimStart('/'); // themes/{uploadSessionId}/...
        var fromPhysical = Path.Combine(root, fromRelative.Replace('/', Path.DirectorySeparatorChar));

        var expectedFromPrefix = $"themes/{uploadSessionId}/";
        /*if (!fromRelative.StartsWith(expectedFromPrefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("URL não pertence à sessão de staging informada.");*/

        var toRelative = $"themes/{themeId}/" + fromRelative.Substring(expectedFromPrefix.Length);
        var toPhysical = Path.Combine(root, toRelative.Replace('/', Path.DirectorySeparatorChar));

        // proteção contra path traversal
        var fullRoot = Path.GetFullPath(root);
        var fullFrom = Path.GetFullPath(fromPhysical);
        var fullTo = Path.GetFullPath(toPhysical);

        if (!fullFrom.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) ||
            !fullTo.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Caminho inválido para promoção de arquivo.");

        Directory.CreateDirectory(Path.GetDirectoryName(fullTo)!);

        if (!System.IO.File.Exists(fullFrom))
            throw new InvalidOperationException("Arquivo de staging não encontrado no servidor.");

        if (System.IO.File.Exists(fullTo))
            System.IO.File.Delete(fullTo);

        System.IO.File.Move(fullFrom, fullTo);

        return $"{publicBase}/{toRelative.Replace('\\', '/')}";
    }


    private static ThemeDto RewriteDtoUrlsPromoted(
        ThemeDto dto,
        Guid uploadSessionId,
        Guid themeId,
        string publicBase,
        string root,
        HashSet<string> slotsFromSession)
    {
        if (slotsFromSession.Count == 0) return dto;

        var newCards = dto.Cards.Select(c =>
        {
            var i = c.OrderIndex;

            // Card main
            var cardSlot = ThemeUploadSlots.CardImageUrl(i);
            var imageUrl = c.ImageUrl;

            if (slotsFromSession.Contains(cardSlot))
                imageUrl = PromoteOneAssetFromSessionToTheme(c.ImageUrl, uploadSessionId, themeId, publicBase, root);

            // ImageQuiz options
            var newImgQuizOptions = c.ImageQuiz.Options.Select((opt, k) =>
            {
                var slot = ThemeUploadSlots.ImageQuizOptionImageUrl(i, k);
                var u = opt.ImageUrl;

                if (slotsFromSession.Contains(slot))
                    u = PromoteOneAssetFromSessionToTheme(opt.ImageUrl, uploadSessionId, themeId, publicBase, root);

                return opt with { ImageUrl = u };
            }).ToList();

            // Correlation items
            var newCorrItems = c.CorrelationQuiz.Items.Select((it, k) =>
            {
                var slot = ThemeUploadSlots.CorrelationItemImageUrl(i, k);
                var u = it.ImageUrl;

                if (slotsFromSession.Contains(slot))
                    u = PromoteOneAssetFromSessionToTheme(it.ImageUrl, uploadSessionId, themeId, publicBase, root);

                return it with { ImageUrl = u };
            }).ToList();

            var newImageQuiz = c.ImageQuiz with { Options = newImgQuizOptions };
            var newCorr = c.CorrelationQuiz with { Items = newCorrItems };

            return c with
            {
                ImageUrl = imageUrl,
                ImageQuiz = newImageQuiz,
                CorrelationQuiz = newCorr
            };
        }).ToList();

        return dto with { Cards = newCards };
    }



    // GET /themes/{id} (para edição no front)
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr)) return Unauthorized();
        var userId = Guid.Parse(userIdStr);

        var theme = await db.Themes
            .AsNoTracking()
            .Include(t => t.EventCards)
                .ThenInclude(c => c.ImageQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.TextQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.TrueOrFalseQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.CorrelationQuiz)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (theme is null) return NotFound();
        if (theme.CreatorUserId != userId) return Forbid();

        var dto = ThemeMapper.ToDto(theme);
        return Ok(dto);
    }

    // DELETE /themes/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> DeleteTheme(
        [FromRoute] Guid id,
        [FromServices] AppDbContext db,
        [FromServices] StorageImageService storageService,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        var theme = await db.Themes
            .Include(t => t.EventCards)
                .ThenInclude(c => c.ImageQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.TextQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.TrueOrFalseQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.CorrelationQuiz)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (theme is null) return NotFound();
        if (theme.CreatorUserId != userId) return Forbid();

        // Remove all related entities
        foreach (var card in theme.EventCards)
        {
            if (card.ImageQuiz != null) db.ImageQuizzes.Remove(card.ImageQuiz);
            if (card.TextQuiz != null) db.TextQuizzes.Remove(card.TextQuiz);
            if (card.TrueOrFalseQuiz != null) db.TrueOrFalseQuizzes.Remove(card.TrueOrFalseQuiz);
            if (card.CorrelationQuiz != null) db.CorrelationQuizzes.Remove(card.CorrelationQuiz);
        }

        db.EventCards.RemoveRange(theme.EventCards);
        db.Themes.Remove(theme);

        await db.SaveChangesAsync(ct);

        // Delete physical files from storage
        storageService.DeleteThemeFolder(id);

        return NoContent();
    }

    // GET /themes/my-themes
    [HttpGet("my-themes")]
    [Authorize(Roles = "teacher")]
    public async Task<IActionResult> GetMyThemes(
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        var userId = User.GetUserId();

        var themes = await db.Themes
            .AsNoTracking()
            .Where(t => t.CreatorUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                id = t.Id,
                name = t.Name,
                image = t.Image,
                readyToPlay = t.ReadyToPlay,
                createdAt = t.CreatedAt,
                cardCount = t.EventCards.Count
            })
            .ToListAsync(ct);

        return Ok(themes);
    }

    // GET /themes/storage
    [HttpGet("storage")]
    [Authorize]
    public async Task<IActionResult> GetThemesStorage(
        [FromServices] AppDbContext db,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 50) pageSize = 50;

        var query = db.Themes
            .AsNoTracking()
            .Include(t => t.CreatorUser)
            .Where(t => t.ReadyToPlay == true);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var themes = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                id = t.Id,
                name = t.Name,
                image = t.Image,
                readyToPlay = t.ReadyToPlay,
                creatorName = t.CreatorUser != null
                    ? (!string.IsNullOrWhiteSpace(t.CreatorUser.FirstName) && !string.IsNullOrWhiteSpace(t.CreatorUser.LastName)
                        ? $"{t.CreatorUser.FirstName} {t.CreatorUser.LastName}".Trim()
                        : t.CreatorUser.FirstName ?? t.CreatorUser.Email)
                    : "Unknown",
                createdAt = t.CreatedAt,
                resume = t.Resume,
                recommendation = t.Recommendation,
                numberOfCards = t.EventCards.Count
            })
            .ToListAsync(ct);

        return Ok(new
        {
            items = themes,
            page,
            pageSize,
            totalCount,
            totalPages
        });
    }

    // GET /themes/{id}/download
    // Endpoint para o jogo Unity baixar o tema completo com todas as cartas e quizzes
    [HttpGet("{id:guid}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadTheme(
        Guid id,
        [FromServices] AppDbContext db,
        CancellationToken ct = default)
    {
        var theme = await db.Themes
            .AsNoTracking()
            .Include(t => t.EventCards)
                .ThenInclude(c => c.ImageQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.TextQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.TrueOrFalseQuiz)
            .Include(t => t.EventCards)
                .ThenInclude(c => c.CorrelationQuiz)
            .Include(t => t.CreatorUser)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (theme == null)
            return NotFound(new { code = "THEME_NOT_FOUND", message = "Tema não encontrado" });

        if (!theme.ReadyToPlay)
            return BadRequest(new { code = "THEME_NOT_READY", message = "Este tema ainda não está pronto para jogar" });

        // Ordena as cartas por OrderIndex e inclui quizzes
        var cards = theme.EventCards
            .OrderBy(c => c.OrderIndex)
            .Select(c => new
            {
                id = c.Id,
                orderIndex = c.OrderIndex,
                year = c.Year,
                era = c.Era.ToString(),
                title = c.Title,
                imageUrl = c.Image,
                // Quiz de Imagens
                imageQuiz = c.ImageQuiz != null ? new
                {
                    question = c.ImageQuiz.Question,
                    options = new[]
                    {
                        new { text = (string?)null, imageUrl = c.ImageQuiz.Image1 },
                        new { text = (string?)null, imageUrl = c.ImageQuiz.Image2 },
                        new { text = (string?)null, imageUrl = c.ImageQuiz.Image3 },
                        new { text = (string?)null, imageUrl = c.ImageQuiz.Image4 }
                    },
                    correctIndex = (int)c.ImageQuiz.CorrectImageIndex
                } : null,
                // Quiz de Texto
                textQuiz = c.TextQuiz != null ? new
                {
                    question = c.TextQuiz.Question,
                    options = new[]
                    {
                        new { text = c.TextQuiz.Text1, imageUrl = (string?)null },
                        new { text = c.TextQuiz.Text2, imageUrl = (string?)null },
                        new { text = c.TextQuiz.Text3, imageUrl = (string?)null },
                        new { text = c.TextQuiz.Text4, imageUrl = (string?)null }
                    },
                    correctIndex = (int)c.TextQuiz.CorrectTextIndex
                } : null,
                // Quiz Verdadeiro ou Falso
                trueFalseQuiz = c.TrueOrFalseQuiz != null ? new
                {
                    statement = c.TrueOrFalseQuiz.Text,
                    answer = c.TrueOrFalseQuiz.IsTrue
                } : null,
                // Quiz de Correlação
                correlationQuiz = c.CorrelationQuiz != null ? new
                {
                    items = new[]
                    {
                        new { imageUrl = c.CorrelationQuiz.Image1, text = c.CorrelationQuiz.Text1 },
                        new { imageUrl = c.CorrelationQuiz.Image2, text = c.CorrelationQuiz.Text2 },
                        new { imageUrl = c.CorrelationQuiz.Image3, text = c.CorrelationQuiz.Text3 }
                    }
                } : null
            })
            .ToList();

        var creatorName = theme.CreatorUser != null
            ? (!string.IsNullOrWhiteSpace(theme.CreatorUser.FirstName) && !string.IsNullOrWhiteSpace(theme.CreatorUser.LastName)
                ? $"{theme.CreatorUser.FirstName} {theme.CreatorUser.LastName}".Trim()
                : theme.CreatorUser.FirstName ?? theme.CreatorUser.Email)
            : "Unknown";

        return Ok(new
        {
            id = theme.Id,
            name = theme.Name,
            version = theme.UpdatedAt.ToString("yyyyMMddHHmmss"),
            creatorName,
            resume = theme.Resume,
            recommendation = theme.Recommendation,
            coverImageUrl = theme.Image,
            cardCount = cards.Count,
            cards
        });
    }

    private static string NormalizePublicUrl(string url, string publicBase)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;

        var u = url.Trim();
        publicBase = (publicBase ?? "").TrimEnd('/');

        // Se vier absoluta (http/https), extrai o path e tenta reduzir para o "publicBase"
        if (Uri.TryCreate(u, UriKind.Absolute, out var abs))
        {
            // Ex: https://host.com/media/themes/... -> /media/themes/...
            u = abs.AbsolutePath;

            // mantém query fora (não deveria existir)
        }

        // Se vier relativa sem começar com "/", corrige
        if (!u.StartsWith("/")) u = "/" + u;

        // Se o publicBase é "/media" e a URL veio "/themes/...", prefixa
        if (!string.IsNullOrWhiteSpace(publicBase) && !u.StartsWith(publicBase + "/", StringComparison.OrdinalIgnoreCase))
        {
            // só prefixa se a URL parece ser do storage (começa com /themes, por ex.)
            // Ajuste aqui se você tiver mais roots públicas.
            if (u.StartsWith("/themes/", StringComparison.OrdinalIgnoreCase))
                u = publicBase + u;
        }

        return u;
    }

}
