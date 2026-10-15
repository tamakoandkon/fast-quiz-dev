using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;

public class MariaDbQuizAttemptRepository(MariaDbQuizDbContext _dbContext) : IQuizAttemptRepository
{
    private DbSet<QuizAttempt> QuizAttempts => _dbContext.QuizAttempts;

    public async Task<IReadOnlyList<QuizAttempt>> GetUnassignedByTemplateIdAsync(
        string templateId, string? cursor, int count)
    {
        var query = QuizAttempts.AsNoTracking().Where(x =>
            x.QuizTemplateId == templateId && x.AssignId == null && x.Status == QuizAttemptStatus.Pending);
        if (!string.IsNullOrWhiteSpace(cursor))
            query = query.Where(x => x.Id.CompareTo(cursor) < 0);
        return await query.OrderByDescending(x => x.Id).Take(count).ToListAsync();
    }

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

        return await query
            .OrderByDescending(x => x.Id)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetAllRecordsByTemplateIdAsync(
        string templateId,
        bool onlyBest,
        string? cursor,
        int count,
        bool isPrevious
    )
    {
        var query = QuizAttempts.AsNoTracking().Where(x => x.QuizTemplateId == templateId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            // 上一页取比游标更新的记录(升序取最近 N 条，再反转回降序)；下一页取更早的记录。
            query = isPrevious
                ? query.Where(x => x.Id.CompareTo(cursor) > 0).OrderBy(x => x.Id)
                : query.Where(x => x.Id.CompareTo(cursor) < 0).OrderByDescending(x => x.Id);
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var attempts = await query.ToListAsync();

        if (onlyBest)
        {
            // 按考生分组，每人只保留最高分(同分取最新的一条)；未分配考生(null)不参与排行。
            attempts =
            [
                .. attempts
                    .Where(x => x.AssignId != null)
                    .GroupBy(x => x.AssignId!)
                    .Select(g =>
                        g.OrderByDescending(x => x.Score).ThenByDescending(x => x.Id).First()
                    )
                    .OrderByDescending(x => x.Id)
                    .Take(count),
            ];
        }
        else
        {
            attempts = [.. attempts.Take(count)];
        }

        if (isPrevious)
        {
            attempts.Reverse();
        }

        return attempts;
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetAllRecordsByUserIdAsync(
        string userId,
        string? templateId,
        bool onlyBest,
        string? cursor,
        int count,
        bool isPrevious,
        string? tenantId = null
    )
    {
        // 只统计已完成的考试(已批阅)：未开始/进行中/待批阅的记录不返回。
        var query = QuizAttempts
            .AsNoTracking()
            .Where(x => x.AssignId == userId && x.Status == QuizAttemptStatus.Graded);

        if (!string.IsNullOrWhiteSpace(tenantId))
            query = query.Where(x => _dbContext.QuizTemplates.Any(t =>
                t.Id == x.QuizTemplateId && t.OwnerId == tenantId));

        if (!string.IsNullOrWhiteSpace(templateId))
        {
            query = query.Where(x => x.QuizTemplateId == templateId);
        }

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            // 上一页取比游标更新的记录(升序取最近 N 条，再反转回降序)；下一页取更早的记录。
            query = isPrevious
                ? query.Where(x => x.Id.CompareTo(cursor) > 0).OrderBy(x => x.Id)
                : query.Where(x => x.Id.CompareTo(cursor) < 0).OrderByDescending(x => x.Id);
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var attempts = await query.ToListAsync();

        if (onlyBest)
        {
            // 按考试模板分组，每场考试只保留该考生最高分(同分取最新的一条)。
            attempts =
            [
                .. attempts
                    .GroupBy(x => x.QuizTemplateId)
                    .Select(g =>
                        g.OrderByDescending(x => x.Score).ThenByDescending(x => x.Id).First()
                    )
                    .OrderByDescending(x => x.Id)
                    .Take(count),
            ];
        }
        else
        {
            attempts = [.. attempts.Take(count)];
        }

        if (isPrevious)
        {
            attempts.Reverse();
        }

        return attempts;
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetAllRecordsByTemplateIdAndUserIdAsync(
        string userId,
        string templateId,
        string? cursor,
        int count,
        bool isPrevious
    )
    {
        // 不过滤状态：进行中/待批阅的记录也要返回，前端才能给出「继续考试」入口。
        var query = QuizAttempts
            .AsNoTracking()
            .Where(x => x.AssignId == userId && x.QuizTemplateId == templateId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            // 上一页取比游标更新的记录(升序取最近 N 条，再反转回降序)；下一页取更早的记录。
            query = isPrevious
                ? query.Where(x => x.Id.CompareTo(cursor) > 0).OrderBy(x => x.Id)
                : query.Where(x => x.Id.CompareTo(cursor) < 0).OrderByDescending(x => x.Id);
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var attempts = await query.Take(count).ToListAsync().ConfigureAwait(false);

        if (isPrevious)
        {
            attempts.Reverse();
        }

        return attempts;
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
