using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.PostgreSql;

[TestFixture]
[Category("PostgreSql")]
internal class PostgreSQLQuizAttemptRepositoryTests : QuizAttemptRepositoryTestsBase
{
    private PostgreSQLQuizDbContext _dbContext = null!;
    private PostgreSQLQuizAttemptRepository _attemptRepository = null!;
    private PostgreSQLQuizTemplateRepository _templateRepository = null!;
    private PostgreSQLQuizSnapshotRepository _quizSnapshotRepository = null!;

    protected override IQuizAttemptRepository Sut => _attemptRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuizSnapshotRepository QuizSnapshots => _quizSnapshotRepository;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await using var setup = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();
        await setup.Database.EnsureDeletedAsync();
        await setup.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Each test already disposes its context; fixture cleanup needs a fresh one.
        await using var cleanup = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();
        await cleanup.Database.EnsureDeletedAsync();
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();

        _attemptRepository = new PostgreSQLQuizAttemptRepository(_dbContext);
        _templateRepository = new PostgreSQLQuizTemplateRepository(_dbContext);
        _quizSnapshotRepository = new PostgreSQLQuizSnapshotRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.AnswerRecords.ExecuteDelete();
        _dbContext.QuestionRecords.ExecuteDelete();
        _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDelete();
        _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
        _dbContext.QuestionSnapshots.ExecuteDelete();
        _dbContext.QuizAttempts.ExecuteDelete();
        _dbContext.QuizSnapshots.ExecuteDelete();
        _dbContext.QuestionSelecionRules.ExecuteDelete();
        _dbContext.QuizTemplates.ExecuteDelete();
        _dbContext.Dispose();
    }
}
