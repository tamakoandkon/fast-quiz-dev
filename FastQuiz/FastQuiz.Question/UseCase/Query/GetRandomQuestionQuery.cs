using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Query;

public record GetRandomQuestionQuery(QuestionType Type, int Level, int Count);
