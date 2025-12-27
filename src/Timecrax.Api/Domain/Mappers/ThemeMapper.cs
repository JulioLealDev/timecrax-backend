using Timecrax.Api.Domain.Entities;
using Timecrax.Api.Domain.Enums;
using Timecrax.Api.Dtos.Theme;

namespace Timecrax.Api.Domain.Mappers;

public static class ThemeMapper
{
    public static Theme ToEntity(ThemeDto dto, Guid creatorUserId, Guid themeId)
    {
        var cards = ToCards(dto.Cards, themeId);
        var readyToPlay = cards.Count >= 12;

        return new Theme
        {
            Id = themeId,
            Name = dto.Name.Trim(),
            Image = dto.Image.Trim(),
            CreatorUserId = creatorUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            ReadyToPlay = readyToPlay,
            EventCards = cards
        };
    }

    public static List<EventCard> ToCards(List<EventCardDto> cards, Guid themeId)
    {
        return cards
            .OrderBy(c => c.OrderIndex)
            .Select(c =>
            {
                var cardId = Guid.NewGuid();

                var entity = new EventCard
                {
                    Id = cardId,
                    ThemeId = themeId,
                    OrderIndex = c.OrderIndex,
                    Title = c.Caption.Trim(),
                    Year = c.Year,
                    Era = Enum.Parse<Era>(c.Era.Trim(), ignoreCase: true),
                    Image = c.ImageUrl.Trim(),
                    CreatedAt = DateTimeOffset.UtcNow,
                };

                entity.ImageQuiz = new ImageQuiz
                {
                    Id = Guid.NewGuid(),
                    EventCardId = cardId,
                    Question = c.ImageQuiz.Question.Trim(),
                    Image1 = c.ImageQuiz.Options[0].ImageUrl.Trim(),
                    Image2 = c.ImageQuiz.Options[1].ImageUrl.Trim(),
                    Image3 = c.ImageQuiz.Options[2].ImageUrl.Trim(),
                    Image4 = c.ImageQuiz.Options[3].ImageUrl.Trim(),
                    CorrectImageIndex = checked((short)c.ImageQuiz.CorrectIndex)
                };

                entity.TextQuiz = new TextQuiz
                {
                    Id = Guid.NewGuid(),
                    EventCardId = cardId,
                    Question = c.TextQuiz.Question.Trim(),
                    Text1 = c.TextQuiz.Options[0].Text.Trim(),
                    Text2 = c.TextQuiz.Options[1].Text.Trim(),
                    Text3 = c.TextQuiz.Options[2].Text.Trim(),
                    Text4 = c.TextQuiz.Options[3].Text.Trim(),
                    CorrectTextIndex = checked((short)c.TextQuiz.CorrectIndex)
                };

                entity.TrueOrFalseQuiz = new TrueOrFalseQuiz
                {
                    Id = Guid.NewGuid(),
                    EventCardId = cardId,
                    Text = c.TrueFalseQuiz.Statement.Trim(),
                    IsTrue = c.TrueFalseQuiz.Answer
                };

                entity.CorrelationQuiz = new CorrelationQuiz
                {
                    Id = Guid.NewGuid(),
                    EventCardId = cardId,
                    Image1 = c.CorrelationQuiz.Items[0].ImageUrl.Trim(),
                    Image2 = c.CorrelationQuiz.Items[1].ImageUrl.Trim(),
                    Image3 = c.CorrelationQuiz.Items[2].ImageUrl.Trim(),
                    Text1 = c.CorrelationQuiz.Items[0].Text.Trim(),
                    Text2 = c.CorrelationQuiz.Items[1].Text.Trim(),
                    Text3 = c.CorrelationQuiz.Items[2].Text.Trim(),
                };

                return entity;
            })
            .ToList();
    }

    // Para edição no front (GET /themes/{id})
    public static ThemeDto ToDto(Theme theme)
    {
        var cards = theme.EventCards
            .OrderBy(c => c.OrderIndex)
            .Select((c, idx) => new EventCardDto(
                OrderIndex: c.OrderIndex,
                Year: c.Year,
                Era: c.Era.ToString(),
                Caption: c.Title,
                ImageUrl: c.Image,
                ImageQuiz: new ImageQuizDto(
                    Question: c.ImageQuiz!.Question,
                    Options: new List<ImageOptionDto>
                    {
                        new(c.ImageQuiz.Image1),
                        new(c.ImageQuiz.Image2),
                        new(c.ImageQuiz.Image3),
                        new(c.ImageQuiz.Image4),
                    },
                    CorrectIndex: c.ImageQuiz.CorrectImageIndex
                ),
                TextQuiz: new TextQuizDto(
                    Question: c.TextQuiz!.Question,
                    Options: new List<TextOptionDto>
                    {
                        new(c.TextQuiz.Text1),
                        new(c.TextQuiz.Text2),
                        new(c.TextQuiz.Text3),
                        new(c.TextQuiz.Text4),
                    },
                    CorrectIndex: c.TextQuiz.CorrectTextIndex
                ),
                TrueFalseQuiz: new TrueFalseQuizDto(
                    Statement: c.TrueOrFalseQuiz!.Text,
                    Answer: c.TrueOrFalseQuiz.IsTrue
                ),
                CorrelationQuiz: new CorrelationQuizDto(
                    Items: new List<CorrelationItemDto>
                    {
                        new(c.CorrelationQuiz.Image1, c.CorrelationQuiz.Text1),
                        new(c.CorrelationQuiz.Image2, c.CorrelationQuiz.Text2),
                        new(c.CorrelationQuiz.Image3, c.CorrelationQuiz.Text3),
                    }
                )
            ))
            .ToList();

        return new ThemeDto(theme.Name, theme.Image,  UploadSessionId: null, cards);
    }
}
