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
        if (role is not ("student" or "teacher"))
            return BadRequest(new { error = "role must be 'student' or 'teacher'." });

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
        if (role == "teacher" && string.IsNullOrWhiteSpace(school))
            return BadRequest(new { error = "schoolName is required for teachers." });

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
