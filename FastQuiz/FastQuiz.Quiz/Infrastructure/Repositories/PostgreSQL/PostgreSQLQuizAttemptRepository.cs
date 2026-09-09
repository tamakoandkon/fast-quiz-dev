using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;

public class PostgreSQLQuizAttemptRepository(PostgreSQLQuizDbContext _dbContext)
    : IQuizAttemptRepository
{
    private DbSet<QuizAttempt> QuizAttempts => _dbContext.QuizAttempts;

    public async Task CreateAsync(QuizAttempt quizAttempt)
    {
        try
        {
            await QuizAttempts.AddAsync(quizAttempt).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuizAttemptCreateException("Quiz attempt create fail.", ex);
        }
    }

    public async Task CreateAsync(IEnumerable<QuizAttempt> quizAttempts)
    {
        try
        {
            await QuizAttempts.AddRangeAsync(quizAttempts).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuizAttemptCreateException("Quiz attempt create fail.", ex);
        }
    }

    public async Task<int> GetAttemptCountAsync(string userId, string templateId)
    {
        return await QuizAttempts
            .AsNoTracking()
            .Where(x => x.AssignId == userId && x.QuizTemplateId == templateId)
            .CountAsync();
    }

    public async Task DeleteByIdAsync(string id)
    {
        await QuizAttempts.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }

    public async Task DeleteByTemplateIdAsync(string templateId)
    {
        await QuizAttempts
            .Where(x => x.QuizTemplateId == templateId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<QuizAttempt?> FindByIdAsync(string id)
    {
        return await QuizAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<QuizAttempt?> FindLastUnfinishedByTemplateIdAndUserIdAsync(
        string userId,
        string templateId
    )
    {
        return await QuizAttempts
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x =>
                x.AssignId == userId
                && x.QuizTemplateId == templateId
                && (x.Status != QuizAttemptStatus.Submitted && x.Status != QuizAttemptStatus.Graded)
            )
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuizAttempt>> FindByQuizTemplateIdAsync(
        string userId,
        string templateId
    )
    {
        return await QuizAttempts
            .AsNoTracking()
            .Where(x => (x.QuizTemplateId == templateId) && (x.AssignId == userId))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetAllByUserIdAsync(
        string userId,
        string? cursor,
        int count
    )
    {
        var query = QuizAttempts.AsNoTracking().Where(x => x.AssignId == userId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            query = query.Where(x => x.Id.CompareTo(cursor) < 0);
        }

        return await query.OrderByDescending(x => x.Id).Take(count).ToListAsync();
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetAllByTemplateIdAsync(
        string templateId,
        string? cursor,
        int count
    )
    {
        var query = QuizAttempts.AsNoTracking().Where(x => x.QuizTemplateId == templateId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            query = query.Where(x => x.Id.CompareTo(cursor) < 0);
        }

        return await query.OrderByDescending(x => x.Id).Take(count).ToListAsync();
    }

    public async Task UpdateAsync(QuizAttempt quizAttempt)
    {
        await QuizAttempts
            .Where(x => x.Id == quizAttempt.Id)
            .ExecuteUpdateAsync(q =>
                q.SetProperty(x => x.AssignId, quizAttempt.AssignId)
                    .SetProperty(x => x.Status, quizAttempt.Status)
                    .SetProperty(x => x.StartTime, quizAttempt.StartTime)
                    .SetProperty(x => x.SubmitTime, quizAttempt.SubmitTime)
                    .SetProperty(x => x.Score, quizAttempt.Score)
            )
            .ConfigureAwait(false);
    }
}
