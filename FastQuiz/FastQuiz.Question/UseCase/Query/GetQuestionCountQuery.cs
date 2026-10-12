using FastQuiz.Question.Domain.Entities;

namespace FastQuiz.Question.UseCase.Query;

/// <summary>
///查询题目数量的查询对象
/// </summary>
public record GetQuestionCountQuery(QuestionType Type, int Level);
