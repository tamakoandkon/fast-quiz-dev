using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.UseCase.Dto;

namespace FastQuiz.Question.Domain.Repositories;

public interface IQuestionGroupRepository
{
    Task CreateAsync(QuestionGroup group);
    Task<QuestionGroupDto?> FindDtoByIdAsync(string id);
    Task<QuestionGroup?> FindByIdAsync(string id);
    Task<IReadOnlyList<QuestionGroup>> FindByIdAsync(IReadOnlyList<string> ids);
    Task<IReadOnlyList<QuestionGroupDto>> GetAllAsync(
        string ownerId,
        string? cursor,
        int count,
        bool isPrevious,
        string? namePrefix
    );
    Task<int> GetCountByGroupIdAsync(string groupId, QuestionType type, int level);
    /// <summary>
    /// 按题库统计题目总数、各题型数量与各难度数量。
    /// 注意：Type 是必填筛选条件，所以难度分项只能基于题库全量题目分组得到。
    /// </summary>
    Task<QuestionGroupStatisticsDto> GetStatisticsByGroupIdAsync(string groupId);
    Task UpdateAsync(QuestionGroup group);
    Task DeleteAsync(string id);
    Task DeleteAsync(IReadOnlyList<string> ids);
}
