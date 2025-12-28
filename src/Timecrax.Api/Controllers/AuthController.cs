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

    public AuthController(AppDbContext db, TokenService tokens, IConfiguration config)
    {
        _db = db;
        _tokens = tokens;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var role = (req.Role ?? "").Trim().ToLowerInvariant();
        if (role is not ("student" or "teacher" or "player"))
            return BadRequest(new { error = "role must be 'student', 'teacher', or 'player'." });

        var firstName = (req.FirstName ?? "").Trim();
        if (firstName.Length < 2)
            return BadRequest(new { error = "firstName must have at least 2 characters." });

        var lastName = (req.LastName ?? "").Trim();
        if (lastName.Length < 2)
            return BadRequest(new { error = "lastName must have at least 2 characters." });

        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (email.Length < 5 || !email.Contains('@'))
            return BadRequest(new { error = "email is invalid." });

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            return BadRequest(new { error = "password must have at least 8 characters." });

        var school = req.SchoolName?.Trim();

        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
            return Conflict(new { error = "email already in use." });

        var now = DateTimeOffset.UtcNow;

        var user = new User
        {
            Role = role,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = PasswordService.Hash(req.Password),
            SchoolName = string.IsNullOrWhiteSpace(school) ? null : school,
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
            return Unauthorized(new { error = "invalid credentials." });

        if (!PasswordService.Verify(password, user.PasswordHash))
            return Unauthorized(new { error = "invalid credentials." });

        var auth = await IssueTokensAsync(user);
        return Ok(auth);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return BadRequest(new { error = "Please provide a valid email address." });

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        // Security: Don't reveal if user exists or not
        // Always return success to prevent user enumeration
        if (user is null)
            return Ok(new { message = "If an account exists with this email, you will receive password reset instructions." });

        // Generate reset token
        var tokenPlain = TokenService.GenerateRefreshTokenPlain(); // Reuse the random token generator
        var tokenHash = TokenService.Sha256(tokenPlain);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1); // Token valid for 1 hour

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

        // TODO: Send email with reset link containing tokenPlain
        // Example: https://yourapp.com/reset-password?token={tokenPlain}
        // For now, we'll just log it (REMOVE IN PRODUCTION)
        Console.WriteLine($"Password reset token for {email}: {tokenPlain}");

        return Ok(new { message = "If an account exists with this email, you will receive password reset instructions." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest(new { error = "Reset token is required." });

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
            return BadRequest(new { error = "Password must be at least 8 characters." });

        var tokenHash = TokenService.Sha256(req.Token);

        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (resetToken is null)
            return BadRequest(new { error = "Invalid or expired reset token." });

        // Check if token is expired
        if (resetToken.ExpiresAt < DateTimeOffset.UtcNow)
            return BadRequest(new { error = "Reset token has expired." });

        // Check if token was already used
        if (resetToken.UsedAt is not null)
            return BadRequest(new { error = "Reset token has already been used." });

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

        return Ok(new { message = "Password has been reset successfully." });
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
