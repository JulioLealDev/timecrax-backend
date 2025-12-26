namespace Timecrax.Api.Dtos.Me;

public sealed class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName  { get; set; }
    public string? SchoolName { get; set; }
    public string? Picture { get; set; }
}
