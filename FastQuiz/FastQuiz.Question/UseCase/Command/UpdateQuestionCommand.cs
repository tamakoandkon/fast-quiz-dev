using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Command;

public record UpdateQuestionCommand(
    string Content,
    int Level,
    QuestionType Type,
    IReadOnlyList<CreateQuestionAnswerCommand> Answers,
    string Remark = ""
);
