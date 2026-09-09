using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;

public class SqliteQuestionSelectionRuleRepository(SqliteQuizDbContext _dbContext)
    : IQuestionSelectionRuleRepository
{
    private DbSet<QuestionSelectionRule> Rules => _dbContext.QuestionSelecionRules;

    public async Task CreateAsync(QuestionSelectionRule rule)
    {
        try
        {
            await Rules.AddAsync(rule).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuestionSelectionRuleCreateException("抽题规则创建失败", ex);
        }
    }

    public async Task<IReadOnlyList<QuestionSelectionRule>> GetByQuizTemplateIdAsync(
        string quizTemplateId,
        string? cursor,
        int count
    )
    {
        var query = Rules.AsNoTracking().Where(x => x.QuizeTemplateId == quizTemplateId);

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

    public async Task<IReadOnlyList<QuestionSelectionRule>> GetByQuizTemplateIdAsync(
        string quizTemplateId
    ) =>
        await Rules
            .AsNoTracking()
            .Where(x => x.QuizeTemplateId == quizTemplateId)
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);

    public async Task<QuestionSelectionRule?> GetByIdAsync(string id)
    {
        return await Rules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<QuestionSelectionRule?> GetByTemplateIdAndConditionAsync(
        string templateId,
        string groupId,
        QuestionType type,
        int level
    )
    {
        return await Rules
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.QuizeTemplateId == templateId
                && x.GroupId == groupId
                && x.Type == type
                && x.Level == level
            )
            .ConfigureAwait(false);
    }

    public async Task UpdateAsync(QuestionSelectionRule rule)
    {
        await Rules
            .Where(x => x.Id == rule.Id)
            .ExecuteUpdateAsync(x =>
                x.SetProperty(r => r.Level, rule.Level)
                    .SetProperty(r => r.Type, rule.Type)
                    .SetProperty(r => r.Count, rule.Count)
                    .SetProperty(r => r.Score, rule.Score)
                    .SetProperty(r => r.GroupId, rule.GroupId)
            )
            .ConfigureAwait(false);
    }

    public async Task DeleteByIdAsync(string id)
    {
        await Rules.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }
}
