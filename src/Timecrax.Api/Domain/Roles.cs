namespace Timecrax.Api.Domain;

/// <summary>
/// User role constants used across the API.
/// </summary>
public static class Roles
{
    public const string Student = "student";
    public const string Teacher = "teacher";
    public const string Player = "player";

    public static readonly string[] All = [Student, Teacher, Player];

    public static bool IsValid(string? role) =>
        !string.IsNullOrWhiteSpace(role) && All.Contains(role.ToLowerInvariant());
}
