namespace FastQuiz.Question.UseCase.Command;

public record DeleteQuestionGroupCommand(IReadOnlyList<string> GroupIds);
