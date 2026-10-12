using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Question.UseCase.Query;

namespace FastQuiz.Question.UseCase.Services;

public interface IQuestionGroupService
{
    Task<QuestionGroupDto> CreateQuestionGroupAsync(
        string userId,
        string tenantId,
        CreateQuestionGroupCommand command
    );
    Task<QuestionGroupDto> GetQuestionGroupByIdAsync(
        string userId,
        string tenantId,
        string groupId
    );
    Task<int> GetQuestionCountByIdAsync(
        string userId,
        string tenantId,
        string groupId,
        GetQuestionCountQuery query
    );
    Task<QuestionGroupStatisticsDto> GetQuestionGroupStatisticsAsync(
        string userId,
        string tenantId,
        string groupId
    );
    Task<IReadOnlyList<QuestionGroupDto>> GetAllQuestionGroupAsync(
        string userId,
        string tenantId,
        GetAllQuestionGroupQuery query
    );
    Task UpdateQuestionGroupAsync(
        string userId,
        string tenantId,
        string groupId,
        UpdateQuestionGroupCommand command
    );
    Task DeleteQuestionGroupByIdAsync(string userId, string tenantId, string groupId);
    Task DeleteQuestionGroupByIdAsync(string userId, string tenantId, IReadOnlyList<string> idList);
}
