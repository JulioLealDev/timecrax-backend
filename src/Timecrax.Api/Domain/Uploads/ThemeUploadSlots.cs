namespace Timecrax.Api.Domain.Uploads;

public static class ThemeUploadSlots
{
    public static string CardImageUrl(int orderIndex) =>
        $"cards[{orderIndex}].imageUrl";

    public static string ImageQuizOptionImageUrl(int orderIndex, int optionIndex) =>
        $"cards[{orderIndex}].imageQuiz.options[{optionIndex}].imageUrl";

    public static string CorrelationItemImageUrl(int orderIndex, int itemIndex) =>
        $"cards[{orderIndex}].correlationQuiz.items[{itemIndex}].imageUrl";

}
