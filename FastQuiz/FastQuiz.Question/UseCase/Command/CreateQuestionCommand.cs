using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Command;

public record CreateQuestionCommand(
    string Content,
    int Level,
    QuestionType Type,
    IReadOnlyList<CreateQuestionAnswerCommand> Answers,
    string? ParentId = null,
    string Remark = ""
);
