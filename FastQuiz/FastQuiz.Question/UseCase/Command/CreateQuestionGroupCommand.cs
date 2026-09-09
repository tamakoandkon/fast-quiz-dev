namespace FastQuiz.Question.UseCase.Command;

public record CreateQuestionGroupCommand(string Name, string Remark = "");
