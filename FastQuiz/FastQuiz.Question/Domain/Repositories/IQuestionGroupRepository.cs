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
    Task UpdateAsync(QuestionGroup group);
    Task DeleteAsync(string id);
    Task DeleteAsync(IReadOnlyList<string> ids);
}
