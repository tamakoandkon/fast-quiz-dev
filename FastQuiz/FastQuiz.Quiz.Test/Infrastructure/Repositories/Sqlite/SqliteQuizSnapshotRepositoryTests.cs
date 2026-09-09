using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Sqlite;

internal class SqliteQuizSnapshotRepositoryTests : QuizSnapshotRepositoryTestsBase
{
    SqliteQuizDbContext _dbContext = null!;
    SqliteQuizSnapshotRepository _quizSnapshotRepository = null!;
    SqliteQuizTemplateRepository _templateRepository = null!;
    SqliteQuizAttemptRepository _attemptRepository = null!;

    SqliteConnection _connection;

    protected override IQuizSnapshotRepository Sut => _quizSnapshotRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuizAttemptRepository Attempts => _attemptRepository;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        (_connection, _dbContext) = InitialTestDbHelper.GetTestSqliteQuizDbContext();

        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            _dbContext?.Dispose();
            _connection?.Dispose();
        }
    }

    [SetUp]
    public void Setup()
    {
        (_connection, _dbContext) = InitialTestDbHelper.GetTestSqliteQuizDbContext();
        _connection.Open();
        _dbContext.Database.EnsureCreated();

        _quizSnapshotRepository = new SqliteQuizSnapshotRepository(_dbContext);
        _templateRepository = new(_dbContext);
        _attemptRepository = new(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.QuizAttempts.ExecuteDelete();
        _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDelete();
        _dbContext.QuizSnapshots.ExecuteDelete();
        _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
        _dbContext.QuestionSnapshots.ExecuteDelete();
        _dbContext.QuestionSelecionRules.ExecuteDelete();
        _dbContext.QuizTemplates.ExecuteDelete();
        _dbContext?.Dispose();
        _connection?.Dispose();
    }
}
