using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;
using Timecrax.Api.Dtos.Me;
using Timecrax.Api.Extensions;
using Timecrax.Api.Services;
using SixLabors.ImageSharp;
using System.Diagnostics;
using SixLabors.ImageSharp.Formats.Webp;

namespace Timecrax.Api.Controllers;

[ApiController]
[Authorize]
[Route("me")]
public class MeController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public MeController(AppDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;

    }

    // (Opcional) GET /me -> para o app carregar perfil
    [HttpGet]
    public async Task<ActionResult<MeResponse>> GetMe()
    {
        var userId = User.GetUserId();

        var user = await _db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Unauthorized(new { error = "user not found." });

        // Busca todos os achievements e verifica quais o usuário tem
        var allAchievements = await _db.Achievements.AsNoTracking().OrderBy(a => a.Image).ToListAsync();
        var userAchievements = await _db.UserAchievements
            .AsNoTracking()
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var achievementDtos = allAchievements.Select(a =>
        {
            var userAch = userAchievements.FirstOrDefault(ua => ua.AchievementId == a.Id);
            return new AchievementDto(
                a.Id,
                a.Name,
                a.Image,
                a.Description,
                userAch?.AchievedAt
            );
        }).ToList();

        // Busca a medal atual do usuário baseada no score
        var currentMedal = await _db.Medals
            .AsNoTracking()
            .Where(m => m.MinScore <= user.Score)
            .OrderByDescending(m => m.MinScore)
            .FirstOrDefaultAsync();

        var currentMedalDto = currentMedal != null ? new MedalDto(
            currentMedal.Id,
            currentMedal.Name,
            currentMedal.Image,
            currentMedal.MinScore
        ) : null;

        // Busca os temas completados pelo usuário
        var completedThemes = await _db.UserCompletedThemes
            .AsNoTracking()
            .Include(uct => uct.Theme)
            .Where(uct => uct.UserId == userId)
            .OrderByDescending(uct => uct.CompletedAt)
            .Select(uct => new CompletedThemeDto(
                uct.Theme.Id,
                uct.Theme.Name,
                uct.Theme.Image,
                uct.CompletedAt
            ))
            .ToListAsync();

        return Ok(new MeResponse(
            user.Id,
            user.Role,
            user.FirstName,
            user.LastName,
            user.Email,
            user.SchoolName,
            user.Picture,
            user.Score,
            user.CreatedAt,
            user.UpdatedAt,
            achievementDtos,
            currentMedalDto,
            completedThemes
        ));
    }

    // PUT /me/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var userId = User.GetUserId();

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Unauthorized(new { error = "user not found." });

        // Validações (ajuste conforme regras do seu negócio)
        if (req.FirstName is not null)
        {
            var first = req.FirstName.Trim();
            if (first.Length < 2) return BadRequest(new { error = "firstName must have at least 2 characters." });
            user.FirstName = first;
        }

        if (req.LastName is not null)
        {
            var last = req.LastName.Trim();
            if (last.Length < 2) return BadRequest(new { error = "lastName must have at least 2 characters." });
            user.LastName = last;
        }

        if (req.SchoolName is not null)
        {
            var school = req.SchoolName.Trim();
            // Exemplo: teacher precisa de escola
            if (user.Role == "teacher" && string.IsNullOrWhiteSpace(school))
                return BadRequest(new { error = "schoolName is required for teachers." });

            user.SchoolName = string.IsNullOrWhiteSpace(school) ? null : school;
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // PUT /me/password
    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(req.CurrentPassword))
            return BadRequest(new { code = "CURRENT_PASSWORD_REQUIRED" });

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
            return BadRequest(new { code = "NEW_PASSWORD_TOO_SHORT" });

        var user = await _db.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Unauthorized(new { code = "USER_NOT_FOUND" });

        if (!PasswordService.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest(new { code = "INVALID_CURRENT_PASSWORD" });

        // Evitar "trocar para a mesma senha"
        if (PasswordService.Verify(req.NewPassword, user.PasswordHash))
            return BadRequest(new { code = "SAME_PASSWORD" });

        user.PasswordHash = PasswordService.Hash(req.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        // Segurança: invalidar refresh tokens existentes após troca de senha
        var now = DateTimeOffset.UtcNow;
        foreach (var rt in user.RefreshTokens)
        {
            if (rt.RevokedAt is null)
                rt.RevokedAt = now;
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // PUT /me/email
    [HttpPut("email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest req)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(req.CurrentPassword))
            return BadRequest(new { code = "CURRENT_PASSWORD_REQUIRED" });

        if (string.IsNullOrWhiteSpace(req.NewEmail))
            return BadRequest(new { code = "NEW_EMAIL_REQUIRED" });

        var newEmailTrimmed = req.NewEmail.Trim().ToLowerInvariant();

        // Validar formato de email
        if (!newEmailTrimmed.IsValidEmail())
            return BadRequest(new { code = "INVALID_EMAIL" });

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Unauthorized(new { code = "USER_NOT_FOUND" });

        // Verificar senha atual
        if (!PasswordService.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest(new { code = "INVALID_CURRENT_PASSWORD" });

        // Evitar "trocar para o mesmo email"
        if (user.Email.Equals(newEmailTrimmed, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { code = "SAME_EMAIL" });

        // Verificar se o novo email já está em uso
        var emailExists = await _db.Users.AnyAsync(u => u.Email == newEmailTrimmed && u.Id != userId);
        if (emailExists)
            return BadRequest(new { code = "EMAIL_IN_USE" });

        user.Email = newEmailTrimmed;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("picture")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB
    public async Task<IActionResult> UploadPicture([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Arquivo inválido." });

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Apenas imagens são permitidas." });

        var userId = User.GetUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Unauthorized();

        var storageRoot = _config["Storage:RootPath"];
        var publicBase = (_config["Storage:PublicBasePath"] ?? "/media").TrimEnd('/');

        if (string.IsNullOrWhiteSpace(storageRoot))
            return StatusCode(500, new { error = "Storage RootPath não configurado." });

        var uploadsDir = Path.Combine(storageRoot, "profile");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"user_{user.Id}.webp";
        var filePath = Path.Combine(uploadsDir, fileName);

        SixLabors.ImageSharp.Image img;
        try
        {
            img = Image.Load(file.OpenReadStream());
        }
        catch
        {
            return BadRequest(new { error = "Imagem inválida ou formato não suportado." });
        }

        using (img)
        {
            if (img.Width < 128 || img.Height < 128)
                return BadRequest(new { error = "Imagem muito pequena." });

            await using var fs = System.IO.File.Create(filePath);
            img.SaveAsWebp(fs, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 75 });
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var pictureUrl = $"{baseUrl}{publicBase}/profile/{fileName}";

        user.Picture = pictureUrl;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(new { picture = pictureUrl });
    }

    // GET /me/ranking
    [HttpGet("ranking")]
    public async Task<IActionResult> GetRanking(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var totalCount = await _db.Users.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.Score)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                id = u.Id,
                name = !string.IsNullOrWhiteSpace(u.FirstName) && !string.IsNullOrWhiteSpace(u.LastName)
                    ? $"{u.FirstName} {u.LastName}".Trim()
                    : u.FirstName ?? u.Email,
                score = u.Score
            })
            .ToListAsync(ct);

        return Ok(new
        {
            items = users,
            page,
            pageSize,
            totalCount,
            totalPages
        });
    }

    // DELETE /me/account
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { code = "PASSWORD_REQUIRED" });

        var user = await _db.Users
            .SingleOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Unauthorized(new { code = "USER_NOT_FOUND" });

        // Verificar senha
        if (!PasswordService.Verify(req.Password, user.PasswordHash))
            return BadRequest(new { code = "INVALID_PASSWORD" });

        // Deletar temas criados pelo usuário antes de deletar a conta
        var createdThemes = await _db.Themes
            .Where(t => t.CreatorUserId == userId)
            .ToListAsync(ct);

        if (createdThemes.Any())
        {
            _db.Themes.RemoveRange(createdThemes);
        }

        // Deletar o usuário (cascade delete cuidará das demais relações)
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

}
