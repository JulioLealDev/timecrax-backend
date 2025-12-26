using System.Security.Claims;

namespace Timecrax.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {

        // Fallback comum no ASP.NET
        var nameId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(nameId) && Guid.TryParse(nameId, out var idFromNameId))
            return idFromNameId;

        throw new UnauthorizedAccessException("User id claim not found.");
    }
}
