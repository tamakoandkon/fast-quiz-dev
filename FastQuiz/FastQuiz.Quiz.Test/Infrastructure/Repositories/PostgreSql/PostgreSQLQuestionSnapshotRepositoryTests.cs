using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.PostgreSql;

[TestFixture]
[Category("PostgreSql")]
internal class PostgreSQLQuestionSnapshotRepositoryTests : QuestionSnapshotRepositoryTestsBase
{
    private PostgreSQLQuizDbContext _dbContext = null!;
    private PostgreSQLQuestionSnapshotRepository _questionSnapshotRepository = null!;
    private PostgreSQLQuizTemplateRepository _templateRepository = null!;

    protected override IQuestionSnapshotRepository Sut => _questionSnapshotRepository;

    protected override IQuizTemplateRepository TemplateRepository => _templateRepository;

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
    public Task Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();

        _questionSnapshotRepository = new PostgreSQLQuestionSnapshotRepository(_dbContext);
        _templateRepository = new PostgreSQLQuizTemplateRepository(_dbContext);
        return Task.CompletedTask;
    }

    [TearDown]
    public async Task TearDown()
    {
        await ClearAllTablesAsync();
        await _dbContext.DisposeAsync();
    }

    #region Utils

    protected override async Task<List<QuestionAnswerSnapshot>> GetQuestionAnswerSnapshotsAsync(
        string questionId
    )
    {
        return await _dbContext
            .QuestionAnswerSnapshots.AsNoTracking()
            .Where(x => x.QuestionSnapshotId == questionId)
            .ToListAsync();
    }

    #endregion

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
