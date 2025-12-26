namespace Timecrax.Api.Dtos.Theme;
public record ThemeDto
(
    string Name,
    string Image,
    Guid? UploadSessionId,
    List<EventCardDto> Cards
);
