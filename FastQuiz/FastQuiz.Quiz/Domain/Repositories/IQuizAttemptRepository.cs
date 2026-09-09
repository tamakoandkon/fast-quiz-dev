using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuizAttemptRepository
{
    Task CreateAsync(QuizAttempt quizAttempt);
    Task CreateAsync(IEnumerable<QuizAttempt> quizAttempts);
    Task<int> GetAttemptCountAsync(string userId, string templateId);
    Task<QuizAttempt?> FindByIdAsync(string id);
    Task<QuizAttempt?> FindLastUnfinishedByTemplateIdAndUserIdAsync(
        string userId,
        string templateId
    );
    Task<IReadOnlyList<QuizAttempt>> FindByQuizTemplateIdAsync(string userId, string templateId);
    Task<IReadOnlyList<QuizAttempt>> GetAllByUserIdAsync(string userId, string? cursor, int count);
    Task<IReadOnlyList<QuizAttempt>> GetAllByTemplateIdAsync(
        string templateId,
        string? cursor,
        int count
    );
    Task UpdateAsync(QuizAttempt quizAttempt);
    Task DeleteByIdAsync(string id);
    Task DeleteByTemplateIdAsync(string templateId);
}
