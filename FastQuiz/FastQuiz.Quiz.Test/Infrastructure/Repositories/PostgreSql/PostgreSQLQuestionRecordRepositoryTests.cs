using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.PostgreSql;

[TestFixture]
[Category("PostgreSql")]
internal class PostgreSQLQuestionRecordRepositoryTests : QuestionRecordRepositoryTestsBase
{
    private PostgreSQLQuizDbContext _dbContext = null!;
    private PostgreSQLQuestionRecordRepository _recordRepository = null!;
    private PostgreSQLQuizTemplateRepository _templateRepository = null!;
    private PostgreSQLQuestionSnapshotRepository _questionSnapshotRepository = null!;
    private PostgreSQLQuizSnapshotRepository _quizSnapshotRepository = null!;
    private PostgreSQLQuizAttemptRepository _quizAttemptRepository = null!;

    protected override IQuestionRecordRepository Sut => _recordRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuestionSnapshotRepository QuestionSnapshots => _questionSnapshotRepository;

    protected override IQuizSnapshotRepository QuizSnapshots => _quizSnapshotRepository;

    protected override IQuizAttemptRepository QuizAttempts => _quizAttemptRepository;

    protected override async Task<
        IReadOnlyList<AnswerRecord>
    > GetAnswerRecordsByQuestionRecordIdAsync(string questionRecordId)
    {
        return await _dbContext
            .AnswerRecords.AsNoTracking()
            .Where(x => x.QuestionRecordId == questionRecordId)
            .ToListAsync();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            await ClearAllTablesAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();

        _recordRepository = new PostgreSQLQuestionRecordRepository(_dbContext);
        _templateRepository = new PostgreSQLQuizTemplateRepository(_dbContext);
        _questionSnapshotRepository = new PostgreSQLQuestionSnapshotRepository(_dbContext);
        _quizSnapshotRepository = new PostgreSQLQuizSnapshotRepository(_dbContext);
        _quizAttemptRepository = new PostgreSQLQuizAttemptRepository(_dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        await ClearAllTablesAsync();
        await _dbContext.DisposeAsync();
    }

    private async Task ClearAllTablesAsync()
    {
        await _dbContext.AnswerRecords.ExecuteDeleteAsync();
        await _dbContext.QuestionRecords.ExecuteDeleteAsync();
        await _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDeleteAsync();
        await _dbContext.QuestionAnswerSnapshots.ExecuteDeleteAsync();
        await _dbContext.QuestionSnapshots.ExecuteDeleteAsync();
        await _dbContext.QuizAttempts.ExecuteDeleteAsync();
        await _dbContext.QuizSnapshots.ExecuteDeleteAsync();
        await _dbContext.QuestionSelecionRules.ExecuteDeleteAsync();
        await _dbContext.QuizTemplates.ExecuteDeleteAsync();
    }
}
