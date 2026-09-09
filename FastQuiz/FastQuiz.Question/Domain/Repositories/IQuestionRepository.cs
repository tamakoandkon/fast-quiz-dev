using FastQuiz.Question.Domain.Entities;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Domain.Repositories;

public interface IQuestionRepository
{
    Task CreateAsync(Q question);
    Task<Q?> FindByIdAsync(string id);
    Task<IReadOnlyList<Q>> GetByIdAsync(IEnumerable<string> ids);
    Task<IReadOnlyList<Q>> GetAllAsync(
        string groupId,
        QuestionType? type,
        int level,
        string? cursor,
        int count,
        bool isPrevious
    );
    Task<IReadOnlyList<string>> GetQuestionIdsAsync(string groupId, QuestionType type, int level);
    Task UpdateAsync(Q question);
    Task DeleteAsync(string id);
}
