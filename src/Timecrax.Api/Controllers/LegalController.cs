using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;

namespace Timecrax.Api.Controllers;

[ApiController]
[Route("legal")]
public class LegalController : ControllerBase
{
    private readonly AppDbContext _db;

    public LegalController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("gdpr/{language}")]
    public async Task<IActionResult> GetGdpr(string language)
    {
        var gdpr = await _db.Gdprs
            .Where(g => g.Language == language)
            .Select(g => new { g.Terms, g.Version })
            .FirstOrDefaultAsync();

        if (gdpr == null)
        {
            // Fallback to English if language not found
            gdpr = await _db.Gdprs
                .Where(g => g.Language == "en")
                .Select(g => new { g.Terms, g.Version })
                .FirstOrDefaultAsync();
        }

        if (gdpr == null)
            return NotFound(new { error = "GDPR terms not found." });

        return Ok(gdpr);
    }
}
