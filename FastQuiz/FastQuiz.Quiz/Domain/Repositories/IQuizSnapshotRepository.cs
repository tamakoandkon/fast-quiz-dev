using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuizSnapshotRepository
{
    Task CreateAsync(QuizSnapshot quizSnapshot);
    Task<QuizSnapshot?> FindByIdAsync(string id);
    Task<string?> FindMinAttemptCountAsync(string templateId);
    Task<QuizSnapshot?> GetFirstSnapshotByTemplateIdAsync(string templateId);
    Task<IReadOnlyList<QuizSnapshot>> GetAllAsync(string quizTemplate, string? cursor, int count);
    Task DeleteByIdAsync(string id);
    Task DeleteByQuizTemplateIdAsync(string templateId);
}
