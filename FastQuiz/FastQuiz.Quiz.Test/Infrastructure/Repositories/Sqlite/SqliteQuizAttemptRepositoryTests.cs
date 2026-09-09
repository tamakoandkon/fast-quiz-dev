using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Sqlite;

internal class SqliteQuizAttemptRepositoryTests : QuizAttemptRepositoryTestsBase
{
    SqliteQuizDbContext _dbContext = null!;
    SqliteQuizAttemptRepository _attemptRepository = null!;
    SqliteQuizTemplateRepository _templateRepository = null!;
    SqliteQuizSnapshotRepository _quizSnapshotRepository = null!;

    SqliteConnection _connection;

    protected override IQuizAttemptRepository Sut => _attemptRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuizSnapshotRepository QuizSnapshots => _quizSnapshotRepository;

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
        var settings = DefaultSqliteQuizDbContextSetting.GetDefault();

        _connection = new SqliteConnection(settings.ConnectionString);
        _connection.Open();

        var options = new DbContextOptionsBuilder<SqliteQuizDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SqliteQuizDbContext(options, Options.Create(settings));
        _dbContext.Database.EnsureCreated();

        _attemptRepository = new SqliteQuizAttemptRepository(_dbContext);
        _templateRepository = new SqliteQuizTemplateRepository(_dbContext);
        _quizSnapshotRepository = new(_dbContext);
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
