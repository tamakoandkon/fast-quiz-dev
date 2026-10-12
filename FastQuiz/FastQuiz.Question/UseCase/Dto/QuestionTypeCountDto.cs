using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Dto;

public record QuestionTypeCountDto(QuestionType Type, int Count);
