namespace FastQuiz.Quiz.UseCase.Queries;

public record GetAllQuestionRecordDtoQuery(string? Cursor = null, int Count = 30);
