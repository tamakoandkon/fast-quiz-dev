using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuizTemplateRepository
{
    Task CreateAsync(QuizTemplate quizTemplate);
    Task<QuizTemplate?> FindByIdAsync(string id);
    Task<IReadOnlyList<QuizTemplate>> FindByIdAsync(IEnumerable<string> ids);
    Task<IReadOnlyList<QuizTemplate>> GetAllAsync(
        string ownerId,
        string? cursor,
        int count,
        bool isPrevious,
        string? namePrefix,
        bool? isPublished
    );
    Task UpdateAsync(QuizTemplate quizTemplate);
    Task DeleteByIdAsync(string id);
}
