using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;

public class SqliteQuizTemplateRepository(SqliteQuizDbContext _dbContext) : IQuizTemplateRepository
{
    private DbSet<QuizTemplate> QuizTemplates => _dbContext.QuizTemplates;

    public async Task CreateAsync(QuizTemplate quizTemplate)
    {
        try
        {
            await QuizTemplates.AddAsync(quizTemplate).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuizTemplateCreateException("试卷模板创建失败", ex);
        }
    }

    public async Task<QuizTemplate?> FindByIdAsync(string id)
    {
        return await QuizTemplates
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuizTemplate>> FindByIdAsync(IEnumerable<string> ids)
    {
        return await QuizTemplates
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuizTemplate>> GetAllAsync(
        string ownerId,
        string? cursor,
        int count,
        bool isPrevious,
        string? namePrefix,
        bool? isPublished
    )
    {
        var q = QuizTemplates.AsNoTracking().Where(x => x.OwnerId == ownerId);

        if (isPublished != null)
        {
            q = q.Where(x => x.IsPublished == isPublished.Value);
        }

        if (!string.IsNullOrWhiteSpace(namePrefix))
        {
            q = q.Where(x => x.Name.StartsWith(namePrefix));
        }

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (isPrevious)
            {
                q = q.Where(x => x.Id.CompareTo(cursor) > 0).OrderBy(x => x.Id);
            }
            else
            {
                q = q.Where(x => x.Id.CompareTo(cursor) < 0).OrderByDescending(x => x.Id);
            }
        }
        else
        {
            q = q.OrderByDescending(x => x.Id);
        }

        var result = await q.Take(count).ToListAsync();

        if (isPrevious)
        {
            result.Reverse();
        }

        return result;
    }

    public async Task UpdateAsync(QuizTemplate quizTemplate)
    {
        await QuizTemplates
            .Where(x => x.Id == quizTemplate.Id)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(q => q.Name, quizTemplate.Name)
                    .SetProperty(q => q.Description, quizTemplate.Description)
                    .SetProperty(q => q.Remark, quizTemplate.Remark)
                    .SetProperty(q => q.AttemptLimit, quizTemplate.AttemptLimit)
                    .SetProperty(q => q.IsUnlimitedTime, quizTemplate.IsUnlimitedTime)
                    .SetProperty(q => q.BeginTime, quizTemplate.BeginTime)
                    .SetProperty(q => q.EndTime, quizTemplate.EndTime)
                    .SetProperty(q => q.DurationMinutes, quizTemplate.DurationMinutes)
                    .SetProperty(q => q.UpdatedAt, quizTemplate.UpdatedAt)
                    .SetProperty(q => q.IsPublished, quizTemplate.IsPublished)
                    .SetProperty(q => q.UserGroupId, quizTemplate.UserGroupId)
            )
            .ConfigureAwait(false);
    }

    public async Task DeleteByIdAsync(string id)
    {
        await QuizTemplates.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }
}
