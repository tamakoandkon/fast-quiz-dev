using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Commands;

public record UpdateQuestionSelectionRuleCommand(
    string GroupId,
    QuestionType QuestionType,
    int Level,
    int Count,
    int Score
);
