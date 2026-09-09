using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Dtos;

public record QuestionSelectionRuleDto(
    string Id,
    string QuizTemplateId,
    string GroupId,
    QuestionType QuestionType,
    int Level,
    int Count,
    int Score
);
