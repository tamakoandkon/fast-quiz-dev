using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;

public class MariaDbQuestionSnapshotRepository(MariaDbQuizDbContext _dbContext)
    : IQuestionSnapshotRepository
{
    private DbSet<QuestionSnapshot> Questions => _dbContext.QuestionSnapshots;
    private DbSet<QuestionAnswerSnapshot> Answers => _dbContext.QuestionAnswerSnapshots;

    public async Task CreateAsync(QuestionSnapshot questionSnapshot)
    {
        try
        {
            await Questions.AddAsync(questionSnapshot).ConfigureAwait(false);
            await Answers.AddRangeAsync(questionSnapshot.AnswerSnapshots).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuestionSnapshotCreateException("Question snapshot create exception.", ex);
        }
    }

    public async Task CreateAsync(IEnumerable<QuestionSnapshot> questionSnapshots)
    {
        var answers = questionSnapshots.SelectMany(x => x.AnswerSnapshots);
        await Questions.AddRangeAsync(questionSnapshots).ConfigureAwait(false);
        await Answers.AddRangeAsync(answers).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteByTemplateIdAsync(string quizTemplateId)
    {
        await Questions.Where(x => x.QuizTemplateId == quizTemplateId).ExecuteDeleteAsync();
    }

    public async Task<IReadOnlyList<QuestionSnapshot>> FindByQuestionIdAsync(
        string quizTemplateId,
        IEnumerable<string> questionIdList
    )
    {
        var questionIds = questionIdList
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (questionIds.Count == 0 || string.IsNullOrWhiteSpace(quizTemplateId))
            return [];

        var questions = await Questions
            .AsNoTracking()
            .Where(x => x.QuizTemplateId == quizTemplateId && questionIds.Contains(x.QuestionId))
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        if (questions.Count == 0)
            return [];

        var questionSnapshotIds = questions.Select(q => q.Id).ToList();

        var answers = await Answers
            .AsNoTracking()
            .Where(a => questionSnapshotIds.Contains(a.QuestionSnapshotId))
            .ToListAsync()
            .ConfigureAwait(false);

        var answersLookup = answers
            .GroupBy(a => a.QuestionSnapshotId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.SortOrder).ToList());

        foreach (var question in questions)
        {
            question.AnswerSnapshots = answersLookup.GetValueOrDefault(question.Id) ?? [];
        }

        return questions;
    }

    public async Task<QuestionSnapshot?> FindByIdAsync(string id)
    {
        var question = await Questions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (question == null)
        {
            return null;
        }

        var answers = await Answers
            .AsNoTracking()
            .Where(a => a.QuestionSnapshotId == id)
            .OrderBy(a => a.SortOrder)
            .ToListAsync()
            .ConfigureAwait(false);

        question.AnswerSnapshots = answers;

        return question;
    }

    public async Task<IReadOnlyList<QuestionSnapshot>> FindByIdAsync(IEnumerable<string> idList)
    {
        var ids = idList.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var questions = await Questions
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync()
            .ConfigureAwait(false);

        var answers = await Answers
            .AsNoTracking()
            .Where(a => ids.Contains(a.QuestionSnapshotId))
            .OrderBy(a => a.SortOrder)
            .ToListAsync()
            .ConfigureAwait(false);

        var answersLookup = answers
            .GroupBy(a => a.QuestionSnapshotId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var question in questions)
        {
            if (answersLookup.TryGetValue(question.Id, out var questionAnswers))
            {
                question.AnswerSnapshots = questionAnswers;
            }
            else
            {
                question.AnswerSnapshots = [];
            }
        }

        return questions;
    }
}
