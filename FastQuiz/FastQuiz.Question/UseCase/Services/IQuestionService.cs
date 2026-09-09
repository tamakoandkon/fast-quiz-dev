using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Question.UseCase.Query;

namespace FastQuiz.Question.UseCase.Services;

public interface IQuestionService
{
    Task<QuestionDto> CreateQuestionAsync(
        string userId,
        string tenantId,
        string groupId,
        CreateQuestionCommand command
    );
    Task<QuestionDto?> GetQuestionByIdAsync(string userId, string tenantId, string questionId);
    Task<IReadOnlyList<QuestionDto>> GetAllQuestionAsync(
        string userId,
        string tenantId,
        string groupId,
        GetAllQuestionQuery query
    );
    Task<IReadOnlyList<QuestionDto>> GetRandomQuestionAsync(
        string userId,
        string tenantId,
        string groupId,
        GetRandomQuestionQuery query
    );
    Task UpdateQuestionAsync(
        string userId,
        string tenantId,
        string questionId,
        UpdateQuestionCommand command
    );
    Task DeleteByIdAsync(string userId, string tenantId, string questionId);
}
