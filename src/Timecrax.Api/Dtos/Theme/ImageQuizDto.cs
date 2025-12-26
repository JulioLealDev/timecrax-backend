namespace Timecrax.Api.Dtos.Theme;
public record ImageQuizDto
(
    string Question,
    List<ImageOptionDto> Options,
    int CorrectIndex
);

public record ImageOptionDto
(
    string ImageUrl
);
