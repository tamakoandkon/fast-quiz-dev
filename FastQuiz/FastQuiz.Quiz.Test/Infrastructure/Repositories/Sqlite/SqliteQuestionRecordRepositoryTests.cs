using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Sqlite;

internal class SqliteQuestionRecordRepositoryTests : QuestionRecordRepositoryTestsBase
{
    SqliteQuizDbContext _dbContext = null!;
    SqliteQuestionRecordRepository _recordRepository = null!;
    SqliteQuizTemplateRepository _templateRepository = null!;
    SqliteQuestionSnapshotRepository _questionSnapshotRepository = null!;
    SqliteQuizSnapshotRepository _quizSnapshotRepository = null!;
    SqliteQuizAttemptRepository _quizAttemptRepository = null!;

    SqliteConnection _connection;

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

        _recordRepository = new SqliteQuestionRecordRepository(_dbContext);
        _templateRepository = new SqliteQuizTemplateRepository(_dbContext);
        _questionSnapshotRepository = new(_dbContext);
        _quizSnapshotRepository = new(_dbContext);
        _quizAttemptRepository = new(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
        _connection?.Dispose();
    }
}
