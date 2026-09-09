namespace FastQuiz.Quiz.UseCase.Queries;

public record GetAllQuestionSelectionRuleQuery(string? Cursor = null, int Count = 20);
