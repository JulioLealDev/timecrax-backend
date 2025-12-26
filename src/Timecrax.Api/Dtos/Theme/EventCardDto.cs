namespace Timecrax.Api.Dtos.Theme;
public record EventCardDto
(
    int OrderIndex,
    int Year,
    string Era,
    string Caption,
    string ImageUrl,

    ImageQuizDto ImageQuiz,
    TextQuizDto TextQuiz,
    TrueFalseQuizDto TrueFalseQuiz,
    CorrelationQuizDto CorrelationQuiz
);
