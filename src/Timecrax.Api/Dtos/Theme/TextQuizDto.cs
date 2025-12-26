namespace Timecrax.Api.Dtos.Theme;
public record TextQuizDto
(
    string Question,
    List<TextOptionDto> Options,
    int CorrectIndex
);

public record TextOptionDto
(
    string Text
);
