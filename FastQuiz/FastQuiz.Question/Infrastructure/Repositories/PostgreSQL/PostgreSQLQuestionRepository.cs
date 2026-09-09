using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Infrastructure.Repositories.PostgreSQL;

public class PostgreSqlQuestionRepository(PostgreSqlQuestionDbContext _dbContext)
    : IQuestionRepository
{
    private readonly DbSet<Q> _questions = _dbContext.Questions;
    private readonly DbSet<QuestionAnswer> _answers = _dbContext.Answers;

    public async Task CreateAsync(Q question)
    {
        try
        {
            await _questions.AddAsync(question).ConfigureAwait(false);
            await _answers.AddRangeAsync(question.Answers).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new CreateQuestionException("Create question fail.", ex);
        }
    }

    public async Task DeleteAsync(string id)
    {
        await _questions.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }

    public async Task<Q?> FindByIdAsync(string id)
    {
        var result = await _questions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .GroupJoin(
                _answers.AsNoTracking(),
                q => q.Id,
                a => a.QuestionId,
                (q, answers) => new { Question = q, Answers = answers.ToList() }
            )
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (result == null)
        {
            return null;
        }

        result.Question.SetAnswers(result.Answers);
        return result.Question;
    }

    public async Task<IReadOnlyList<Q>> GetByIdAsync(IEnumerable<string> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        var results = await _questions
            .AsNoTracking()
            .Where(x => idList.Contains(x.Id))
            .GroupJoin(
                _answers.AsNoTracking(),
                q => q.Id,
                a => a.QuestionId,
                (q, answers) => new { Question = q, Answers = answers.ToList() }
            )
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var result in results)
        {
            result.Question.SetAnswers(result.Answers);
        }

        return [.. results.Select(r => r.Question)];
    }

    public async Task<IReadOnlyList<Q>> GetAllAsync(
        string groupId,
        QuestionType? type,
        int level,
        string? cursor,
        int count,
        bool isPrevious
    )
    {
        var query = _questions
            .AsNoTracking()
            .Where(x =>
                x.GroupId == groupId
                && (type == null || x.Type == type)
                && (level < 0 || x.Level == level)
            );

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (isPrevious)
            {
                query = query.Where(x => x.Id.CompareTo(cursor) > 0).OrderBy(x => x.Id);
            }
            else
            {
                query = query.Where(x => x.Id.CompareTo(cursor) < 0).OrderByDescending(x => x.Id);
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var questions = await query.Take(count).ToListAsync().ConfigureAwait(false);

        if (questions.Count == 0)
        {
            return [];
        }

        if (isPrevious)
        {
            questions.Reverse();
        }

        var questionIds = questions.Select(q => q.Id).ToList();
        var allAnswers = await _answers
            .AsNoTracking()
            .Where(a => questionIds.Contains(a.QuestionId))
            .ToListAsync()
            .ConfigureAwait(false);

        var answerLookup = allAnswers.ToLookup(a => a.QuestionId);
        foreach (var q in questions)
        {
            q.SetAnswers(answerLookup[q.Id].ToList());
        }

        return questions;
    }

    public async Task<IReadOnlyList<string>> GetQuestionIdsAsync(
        string groupId,
        QuestionType type,
        int level
    )
    {
        return await _questions
            .AsNoTracking()
            .Where(x => x.GroupId == groupId && x.Type == type && x.Level == level)
            .Select(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(Q question)
    {
        await using var transaction = await _dbContext
            .Database.BeginTransactionAsync()
            .ConfigureAwait(false);

        try
        {
            var trackedEntries = _dbContext
                .ChangeTracker.Entries<QuestionAnswer>()
                .Where(e => e.Entity.QuestionId == question.Id)
                .ToList();

            foreach (var entry in trackedEntries)
            {
                entry.State = EntityState.Detached;
            }

            await _answers
                .Where(x => x.QuestionId == question.Id)
                .ExecuteDeleteAsync()
                .ConfigureAwait(false);

            await _questions
                .Where(x => x.Id == question.Id)
                .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(x => x.MarkupContent, question.MarkupContent)
                        .SetProperty(x => x.Remark, question.Remark)
                        .SetProperty(x => x.Level, question.Level)
                        .SetProperty(x => x.Type, question.Type)
                )
                .ConfigureAwait(false);

            await _answers.AddRangeAsync(question.Answers).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            await transaction.CommitAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw new UpdateQuestionException($"Update question fail: {ex.Message}", ex);
        }
    }
}
