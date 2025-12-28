namespace Timecrax.Api.Dtos.Theme;
public record ThemeDto
(
    string Name,
    string? Resume,
    string? Recommendation,
    string Image,
    Guid? UploadSessionId,
    List<EventCardDto> Cards
);
