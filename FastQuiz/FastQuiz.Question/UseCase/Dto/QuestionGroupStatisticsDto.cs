namespace FastQuiz.Question.UseCase.Dto;

public record QuestionGroupStatisticsDto(
    int TotalCount,
    IReadOnlyList<QuestionTypeCountDto> Types,
    IReadOnlyList<QuestionLevelCountDto> Levels
);
