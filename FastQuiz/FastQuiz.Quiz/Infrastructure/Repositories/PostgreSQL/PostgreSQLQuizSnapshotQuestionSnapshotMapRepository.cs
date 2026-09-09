using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;

public class PostgreSQLQuizSnapshotQuestionSnapshotMapRepository(PostgreSQLQuizDbContext _dbContext)
    : IQuizSnapshotQuestionSnapshotMapRepository
{
    private DbSet<QuizSnapshotQuestionSnapshotMap> QuizSnapshotQuestionSnapshotMaps =>
        _dbContext.QuizSnapshotQuestionSnapshotMaps;

    public async Task CreateAsync(IEnumerable<QuizSnapshotQuestionSnapshotMap> maps)
    {
        try
        {
            await QuizSnapshotQuestionSnapshotMaps.AddRangeAsync(maps).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuizSnapshotQuestionSnapshotMapCreateException(
                "Quiz snapshot question snapshot create fail.",
                ex
            );
        }
    }

    public async Task CreateAsync(QuizSnapshotQuestionSnapshotMap map)
    {
        try
        {
            await QuizSnapshotQuestionSnapshotMaps.AddAsync(map).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuizSnapshotQuestionSnapshotMapCreateException(
                "Quiz snapshot question snapshot create fail.",
                ex
            );
        }
    }

    public async Task DeleteByQuizSnapshotIdAsync(string quizSnapshotId)
    {
        await QuizSnapshotQuestionSnapshotMaps
            .Where(x => x.QuizSnapshotId == quizSnapshotId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuizSnapshotQuestionSnapshotMap>> GetAllByQuizSnapshotIdAsync(
        string quizSnapshotId,
        string? cursor,
        int count
    )
    {
        var query = QuizSnapshotQuestionSnapshotMaps
            .AsNoTracking()
            .Where(x => x.QuizSnapshotId == quizSnapshotId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            query = query.Where(x => x.QuestionSnapshotId.CompareTo(cursor) < 0);
        }

        return await query
            .OrderByDescending(x => x.QuestionSnapshotId)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuizSnapshotQuestionSnapshotMap>> GetAllByQuizSnapshotIdAsync(
        string quizSnapshotId
    ) =>
        await QuizSnapshotQuestionSnapshotMaps
            .AsNoTracking()
            .Where(x => x.QuizSnapshotId == quizSnapshotId)
            .OrderByDescending(x => x.QuestionSnapshotId)
            .ToListAsync()
            .ConfigureAwait(false);
}
