namespace Timecrax.Api.Dtos.Auth;

public record RegisterRequest(
    string Role,        // "student" | "teacher"
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? SchoolName
);
