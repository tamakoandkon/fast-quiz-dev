using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Infrastructure.Repositories.Sqlite;

public class SqliteQuestionRepository(SqliteQuestionDbContext _dbContext) : IQuestionRepository
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
        var question = await _questions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);

        if (question == null)
        {
            return null;
        }

        var answers = await _answers
            .AsNoTracking()
            .Where(a => a.QuestionId == id)
            .ToListAsync()
            .ConfigureAwait(false);

        question.SetAnswers(answers);

        return question;
    }

    public async Task<IReadOnlyList<Q>> GetByIdAsync(IEnumerable<string> ids)
    {
        if (!ids.Any())
        {
            return [];
        }

        var idList = ids.ToList();

        var questions = await _questions
            .AsNoTracking()
            .Where(x => idList.Contains(x.Id))
            .ToListAsync()
            .ConfigureAwait(false);

        if (questions.Count == 0)
        {
            return [];
        }

        var questionIds = questions.Select(q => q.Id).ToList();

        var answers = await _answers
            .AsNoTracking()
            .Where(a => questionIds.Contains(a.QuestionId))
            .ToListAsync()
            .ConfigureAwait(false);

        var answersByQuestionId = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var question in questions)
        {
            if (answersByQuestionId.TryGetValue(question.Id, out var questionAnswers))
            {
                question.SetAnswers(questionAnswers);
            }
            else
            {
                question.SetAnswers([]);
            }
        }

        return questions;
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
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.Id)
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
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await _answers.Where(x => x.QuestionId == question.Id).ExecuteDeleteAsync();

            await _questions
                .Where(x => x.Id == question.Id)
                .ExecuteUpdateAsync(entity =>
                    entity
                        .SetProperty(x => x.MarkupContent, question.MarkupContent)
                        .SetProperty(x => x.Remark, question.Remark)
                        .SetProperty(x => x.Level, question.Level)
                        .SetProperty(x => x.Type, question.Type)
                );

            var trackedEntries = _dbContext
                .ChangeTracker.Entries<QuestionAnswer>()
                .Where(e => e.Entity.QuestionId == question.Id)
                .ToList();

            foreach (var entry in trackedEntries)
            {
                entry.State = EntityState.Detached;
            }

            await _answers.AddRangeAsync(question.Answers);

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new UpdateQuestionException("Update question fail." + ex.Message, ex);
        }
    }
}
