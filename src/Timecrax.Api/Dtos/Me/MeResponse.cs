namespace Timecrax.Api.Dtos.Me;

public sealed record MeResponse(
    Guid Id,
    string Role,
    string FirstName,
    string LastName,
    string Email,
    string? SchoolName,
    string? Picture,
    int Score,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
