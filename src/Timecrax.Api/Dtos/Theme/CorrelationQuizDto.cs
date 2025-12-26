namespace Timecrax.Api.Dtos.Theme;

public record CorrelationQuizDto
(
    List<CorrelationItemDto> Items
);

public record CorrelationItemDto
(
    string ImageUrl,
    string Text
);
