using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;
using Timecrax.Api.Domain.Entities;
using Timecrax.Api.Dtos.Auth;
using Timecrax.Api.Services;

namespace Timecrax.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly IConfiguration _config;
    private readonly EmailService _email;

    public AuthController(AppDbContext db, TokenService tokens, IConfiguration config, EmailService email)
    {
        _db = db;
        _tokens = tokens;
        _config = config;
        _email = email;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var role = (req.Role ?? "").Trim().ToLowerInvariant();
        if (role is not ("student" or "teacher" or "player"))
            return BadRequest(new { code = "INVALID_ROLE" });

        var firstName = (req.FirstName ?? "").Trim();
        if (firstName.Length < 2)
            return BadRequest(new { code = "FIRST_NAME_TOO_SHORT" });

        var lastName = (req.LastName ?? "").Trim();
        if (lastName.Length < 2)
            return BadRequest(new { code = "LAST_NAME_TOO_SHORT" });

        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (email.Length < 5 || !email.Contains('@'))
            return BadRequest(new { code = "INVALID_EMAIL" });

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            return BadRequest(new { code = "PASSWORD_TOO_SHORT" });

        var school = req.SchoolName?.Trim();

        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
            return Conflict(new { code = "EMAIL_IN_USE" });

        // Get GDPR version based on language
        var language = (req.Language ?? "en").Trim().ToLowerInvariant();
        var gdprVersion = await _db.Gdprs
            .Where(g => g.Language == language)
            .Select(g => (int?)g.Version)
            .FirstOrDefaultAsync();

        // Fallback to English if language not found
        if (gdprVersion == null)
        {
            gdprVersion = await _db.Gdprs
                .Where(g => g.Language == "en")
                .Select(g => (int?)g.Version)
                .FirstOrDefaultAsync();
        }

        var now = DateTimeOffset.UtcNow;

        var user = new User
        {
            Role = role,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = PasswordService.Hash(req.Password),
            SchoolName = string.IsNullOrWhiteSpace(school) ? null : school,
            GdprVersion = gdprVersion,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // opcional: auto-login após register
        var auth = await IssueTokensAsync(user);
        return Ok(auth);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var password = req.Password ?? "";

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });

        if (!PasswordService.Verify(password, user.PasswordHash))
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });

        var auth = await IssueTokensAsync(user);
        return Ok(auth);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var language = (req.Language ?? "en").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return BadRequest(new { code = "INVALID_EMAIL" });

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        // Security: Don't reveal if user exists or not
        // Always return success to prevent user enumeration
        if (user is null)
            return Ok(new { success = true });

        // Generate reset token
        var tokenPlain = TokenService.GenerateRefreshTokenPlain();
        var tokenHash = TokenService.Sha256(tokenPlain);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            UsedAt = null
        };

        _db.PasswordResetTokens.Add(resetToken);
        await _db.SaveChangesAsync();

        // Send email with reset link
        await _email.SendPasswordResetEmailAsync(email, tokenPlain, language);

        return Ok(new { success = true });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest(new { code = "TOKEN_REQUIRED" });

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
            return BadRequest(new { code = "PASSWORD_TOO_SHORT" });

        var tokenHash = TokenService.Sha256(req.Token);

        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (resetToken is null)
            return BadRequest(new { code = "INVALID_TOKEN" });

        // Check if token is expired
        if (resetToken.ExpiresAt < DateTimeOffset.UtcNow)
            return BadRequest(new { code = "TOKEN_EXPIRED" });

        // Check if token was already used
        if (resetToken.UsedAt is not null)
            return BadRequest(new { code = "TOKEN_ALREADY_USED" });

        // Update user password
        var user = resetToken.User;
        user.PasswordHash = PasswordService.Hash(req.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        // Mark token as used
        resetToken.UsedAt = DateTimeOffset.UtcNow;

        // Revoke all existing refresh tokens for security
        var refreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var rt in refreshTokens)
        {
            rt.RevokedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(new { success = true });
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var (accessToken, accessExpiresAt) = _tokens.CreateAccessToken(user);

        var refreshPlain = TokenService.GenerateRefreshTokenPlain();
        var refreshHash = TokenService.Sha256(refreshPlain);

        var days = int.Parse(_config["Jwt:RefreshTokenDays"]!);
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(days);

        var rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpiresAt,
            RevokedAt = null,
            ReplacedByTokenHash = null
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        return new AuthResponse(accessToken, refreshPlain, accessExpiresAt);
    }
}
