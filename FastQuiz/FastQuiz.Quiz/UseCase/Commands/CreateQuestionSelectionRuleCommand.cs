using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Commands;

public record CreateQuestionSelectionRuleCommand(
    string GroupId,
    QuestionType QuestionType,
    int Level,
    int Count,
    int Score
);
