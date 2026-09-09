using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;

public class PostgreSQLQuizSnapshotRepository(PostgreSQLQuizDbContext _dbContext)
    : IQuizSnapshotRepository
{
    private DbSet<QuizSnapshot> Snapshots => _dbContext.QuizSnapshots;
    private DbSet<QuizAttempt> Attempts => _dbContext.QuizAttempts;

    public async Task CreateAsync(QuizSnapshot quizSnapshot)
    {
        try
        {
            await Snapshots.AddAsync(quizSnapshot).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuizSnapshotCreateException("Quiz snapshot create fail in repository", ex);
        }
    }

    public async Task<QuizSnapshot?> FindByIdAsync(string id)
    {
        return await Snapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<string?> FindMinAttemptCountAsync(string templateId)
    {
        return await Snapshots
            .AsNoTracking()
            .Where(s => s.QuizTemplateId == templateId)
            .Select(s => new { s.Id, AttemptCount = Attempts.Count(a => a.QuizSnapshotId == s.Id) })
            .OrderBy(x => x.AttemptCount)
            .ThenBy(x => x.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<QuizSnapshot?> GetFirstSnapshotByTemplateIdAsync(string templateId)
    {
        return await Snapshots
            .AsNoTracking()
            .Where(x => x.QuizTemplateId == templateId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<QuizSnapshot>> GetAllAsync(
        string quizTemplate,
        string? cursor,
        int count
    )
    {
        var query = Snapshots.AsNoTracking().Where(x => x.QuizTemplateId == quizTemplate);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            query = query.Where(x => x.Id.CompareTo(cursor) < 0);
        }

        return await query
            .OrderByDescending(x => x.Id)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task DeleteByIdAsync(string id)
    {
        await Snapshots.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }

    public async Task DeleteByQuizTemplateIdAsync(string templateId)
    {
        await Snapshots
            .Where(x => x.QuizTemplateId == templateId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }
}
