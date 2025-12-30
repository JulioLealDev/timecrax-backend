using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;
using Timecrax.Api.Domain;
using Timecrax.Api.Domain.Entities;
using Timecrax.Api.Dtos.Auth;
using Timecrax.Api.Extensions;
using Timecrax.Api.Services;

namespace Timecrax.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    // Rate limiting constants
    private const int RegistrationMaxAttempts = 10;
    private const int RegistrationWindowMinutes = 60;
    private const int LoginMaxAttempts = 5;
    private const int LoginWindowMinutes = 15;
    private const int PasswordResetMaxAttempts = 5;
    private const int PasswordResetWindowMinutes = 60;

    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly IConfiguration _config;
    private readonly EmailService _email;
    private readonly RateLimitService _rateLimit;

    public AuthController(AppDbContext db, TokenService tokens, IConfiguration config, EmailService email, RateLimitService rateLimit)
    {
        _db = db;
        _tokens = tokens;
        _config = config;
        _email = email;
        _rateLimit = rateLimit;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        // Rate limiting by IP
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var rateLimitKey = $"register:{ip}";
        if (_rateLimit.IsRateLimited(rateLimitKey, RegistrationMaxAttempts, RegistrationWindowMinutes))
            return StatusCode(429, new { code = "TOO_MANY_REQUESTS" });

        var role = (req.Role ?? "").Trim().ToLowerInvariant();
        if (!Roles.IsValid(role))
            return BadRequest(ErrorResponse.Single(ErrorCodes.InvalidRole));

        var firstName = (req.FirstName ?? "").Trim();
        if (firstName.Length < 2)
            return BadRequest(new { code = "FIRST_NAME_TOO_SHORT" });

        var lastName = (req.LastName ?? "").Trim();
        if (lastName.Length < 2)
            return BadRequest(new { code = "LAST_NAME_TOO_SHORT" });

        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (!email.IsValidEmail())
            return BadRequest(new { code = "INVALID_EMAIL" });

        var passwordValidation = req.Password.ValidatePassword();
        if (!passwordValidation.IsValid)
            return BadRequest(new { code = passwordValidation.ErrorCode });

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

        // Rate limiting by email
        var rateLimitKey = $"login:{email}";
        if (_rateLimit.IsRateLimited(rateLimitKey, LoginMaxAttempts, LoginWindowMinutes))
            return StatusCode(429, new { code = "TOO_MANY_REQUESTS" });

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            _rateLimit.RecordAttempt(rateLimitKey, LoginWindowMinutes);
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });
        }

        if (!PasswordService.Verify(password, user.PasswordHash))
        {
            _rateLimit.RecordAttempt(rateLimitKey, LoginWindowMinutes);
            return Unauthorized(new { code = "INVALID_CREDENTIALS" });
        }

        var auth = await IssueTokensAsync(user);
        return Ok(auth);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        var language = (req.Language ?? "en").Trim().ToLowerInvariant();

        if (!email.IsValidEmail())
            return BadRequest(new { code = "INVALID_EMAIL" });

        // Rate limiting by email
        var rateLimitKey = $"forgot-password:{email}";
        if (_rateLimit.IsRateLimited(rateLimitKey, PasswordResetMaxAttempts, PasswordResetWindowMinutes))
            return StatusCode(429, new { code = "TOO_MANY_REQUESTS" });

        _rateLimit.RecordAttempt(rateLimitKey, PasswordResetWindowMinutes);

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

        var passwordValidation = req.NewPassword.ValidatePassword();
        if (!passwordValidation.IsValid)
            return BadRequest(new { code = passwordValidation.ErrorCode });

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
