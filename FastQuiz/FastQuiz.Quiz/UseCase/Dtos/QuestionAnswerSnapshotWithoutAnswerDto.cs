namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuestionAnswerSnapshotWithoutAnswerDto(
    string Id,
    string MarkUpContent,
    string QuestionSnapshotId,
    int SortOrder
);
