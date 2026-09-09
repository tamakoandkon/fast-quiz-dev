using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Command;

public record CreateQuestionAnswerCommand(
    string Content,
    bool IsCorrect,
    int SortOrder,
    MatchStrategy MatchStrategy
);
