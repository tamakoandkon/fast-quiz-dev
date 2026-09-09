using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;

public class MariaDbQuestionRecordRepository(MariaDbQuizDbContext _dbContext)
    : IQuestionRecordRepository
{
    private DbSet<QuestionRecord> QuestionRecords => _dbContext.QuestionRecords;
    private DbSet<AnswerRecord> AnswerRecords => _dbContext.AnswerRecords;

    public async Task CreateAsync(IEnumerable<QuestionRecord> records)
    {
        try
        {
            await QuestionRecords.AddRangeAsync(records).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new QuestionRecordCreateException("Question record create fail.", ex);
        }
    }

    public async Task DeleteByQuizAttemptId(string quizAttemptId)
    {
        await QuestionRecords
            .Where(x => x.QuizAttemptId == quizAttemptId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task<QuestionRecord?> FindQuestionRecordByIdAsync(string id)
    {
        return await QuestionRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuestionRecord>> GetAllByQuizAttempIdAsync(
        string quizAttemptId,
        string? cursor,
        int count
    )
    {
        var questionQuery = QuestionRecords
            .AsNoTracking()
            .Where(x => x.QuizAttemptId == quizAttemptId);

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            questionQuery = questionQuery.Where(x => x.Id.CompareTo(cursor) < 0);
        }

        var questions = await questionQuery
            .OrderByDescending(x => x.Id)
            .Take(count)
            .ToListAsync()
            .ConfigureAwait(false);

        if (!questions.Any())
        {
            return questions;
        }

        // 获取所有问题ID
        var questionIds = questions.Select(x => x.Id).ToList();

        // 查询 AnswerRecords
        var answerRecords = await AnswerRecords
            .AsNoTracking()
            .Where(x => questionIds.Contains(x.QuestionRecordId))
            .ToListAsync()
            .ConfigureAwait(false);

        // 将 AnswerRecords 和 Answers 分配到对应的 QuestionRecord
        foreach (var question in questions)
        {
            question.AnswerRecords = answerRecords
                .Where(x => x.QuestionRecordId == question.Id)
                .ToList();
        }

        return questions;
    }

    public async Task<IReadOnlyList<QuestionRecord>> GetAllByQuizAttempIdAsync(string quizAttemptId)
    {
        var questionQuery = QuestionRecords
            .AsNoTracking()
            .Where(x => x.QuizAttemptId == quizAttemptId);

        var questions = await questionQuery
            .OrderByDescending(x => x.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        if (questions.Count == 0)
        {
            return questions;
        }

        // 获取所有问题ID
        var questionIds = questions.Select(x => x.Id).ToList();

        // 查询 AnswerRecords
        var answerRecords = await AnswerRecords
            .AsNoTracking()
            .Where(x => questionIds.Contains(x.QuestionRecordId))
            .ToListAsync()
            .ConfigureAwait(false);

        // 将 AnswerRecords 和 Answers 分配到对应的 QuestionRecord
        foreach (var question in questions)
        {
            question.AnswerRecords =
            [
                .. answerRecords.Where(x => x.QuestionRecordId == question.Id),
            ];
        }

        return questions;
    }

    public async Task CreateAnswerRecordAsync(IEnumerable<AnswerRecord> answerRecords)
    {
        await AnswerRecords.AddRangeAsync(answerRecords).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task UpdateAsync(QuestionRecord record)
    {
        await QuestionRecords
            .Where(x => x.Id == record.Id)
            .ExecuteUpdateAsync(q => q.SetProperty(x => x.IsCorrect, record.IsCorrect));
    }

    public async Task SetQuestionRecordsCorrectAsync(IEnumerable<string> idList, bool isCorrect)
    {
        var list = idList.ToList();
        await QuestionRecords
            .Where(x => list.Contains(x.Id))
            .ExecuteUpdateAsync(q => q.SetProperty(x => x.IsCorrect, isCorrect));
    }

    public async Task DeleteAnswerRecordByQuestionRecordIdAsync(string questionRecordId)
    {
        await AnswerRecords
            .Where(x => x.QuestionRecordId == questionRecordId)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }

    public async Task DeleteAnswerRecordByQuestionRecordIdAsync(
        IEnumerable<string> questionRecordIdList
    )
    {
        await AnswerRecords
            .Where(x => questionRecordIdList.Contains(x.QuestionRecordId))
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);
    }
}
