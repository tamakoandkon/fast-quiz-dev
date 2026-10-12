using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.UseCase.Dto;
using Microsoft.EntityFrameworkCore;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Infrastructure.Repositories.Sqlite;

public class SqliteQuestionGroupRepository(SqliteQuestionDbContext _dbContext)
    : IQuestionGroupRepository
{
    private readonly DbSet<QuestionGroup> _groups = _dbContext.QuestionGroups;
    private readonly DbSet<Q> _questions = _dbContext.Questions;

    public async Task CreateAsync(QuestionGroup group)
    {
        try
        {
            await _groups.AddAsync(group).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new CreateQuestionGroupException("Question group create fail.", ex);
        }
    }

    public async Task<QuestionGroupDto?> FindDtoByIdAsync(string id)
    {
        return await _groups
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(g => new QuestionGroupDto(
                g.Id,
                g.OwnerId,
                g.Name,
                g.Remark,
                _questions.Count(q => q.GroupId == g.Id)
            ))
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<QuestionGroup?> FindByIdAsync(string id)
    {
        return await _groups
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuestionGroup>> FindByIdAsync(IReadOnlyList<string> ids)
    {
        return await _groups
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuestionGroupDto>> GetAllAsync(
        string ownerId,
        string? cursor,
        int count,
        bool isPrevious,
        string? namePrefix
    )
    {
        var q = _groups.AsNoTracking().Where(x => x.OwnerId == ownerId);

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

        var result = await q.Take(count)
            .Select(g => new QuestionGroupDto(
                g.Id,
                g.OwnerId,
                g.Name,
                g.Remark,
                _questions.Count(q => q.GroupId == g.Id)
            ))
            .ToListAsync();

        if (isPrevious)
        {
            result.Reverse();
        }

        return result;
    }

    public async Task<int> GetCountByGroupIdAsync(string groupId, QuestionType type, int level)
    {
        return await _questions
            .AsNoTracking()
            .Where(x => x.GroupId == groupId && x.Type == type && x.Level == level)
            .CountAsync();
    }

    public async Task<QuestionGroupStatisticsDto> GetStatisticsByGroupIdAsync(string groupId)
    {
        var typeCounts = await _questions
            .AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .GroupBy(x => x.Type)
            .Select(g => new QuestionTypeCountDto(g.Key, g.Count()))
            .ToListAsync()
            .ConfigureAwait(false);

        var levelCounts = await _questions
            .AsNoTracking()
            .Where(x => x.GroupId == groupId)
            .GroupBy(x => x.Level)
            .Select(g => new QuestionLevelCountDto(g.Key, g.Count()))
            .ToListAsync()
            .ConfigureAwait(false);

        var totalCount = typeCounts.Sum(x => x.Count);

        return new QuestionGroupStatisticsDto(
            totalCount,
            [.. typeCounts.OrderBy(x => x.Type)],
            [.. levelCounts.OrderBy(x => x.Level)]
        );
    }

    public async Task UpdateAsync(QuestionGroup group)
    {
        await _groups
            .Where(x => (x.Id == group.Id))
            .ExecuteUpdateAsync(x =>
                x.SetProperty(g => g.Name, group.Name).SetProperty(g => g.Remark, group.Remark)
            )
            .ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        await _groups.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(IReadOnlyList<string> ids)
    {
        await _groups.Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync().ConfigureAwait(false);
    }
}
