namespace Timecrax.Api.Dtos.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt
);
