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
    DateTimeOffset UpdatedAt,
    List<AchievementDto> Achievements,
    MedalDto? CurrentMedal
);

public sealed record AchievementDto(
    Guid Id,
    string Name,
    string Image,
    string Description,
    DateTimeOffset? UnlockedAt
);

public sealed record MedalDto(
    Guid Id,
    string Name,
    string Image,
    int MinScore
);
