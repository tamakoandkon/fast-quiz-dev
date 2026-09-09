namespace FastQuiz.Quiz.UseCase.Queries;

public record GetPublishedQuizTemplateQuery(string? Cursor = null, int Count = 30);
