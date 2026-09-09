using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Sqlite;

internal class SqliteQuizSnapshotQuestionSnapshotMapRepositoryTests
    : QuizSnapshotQuestionSnapshotMapRepositoryTestsBase
{
    SqliteQuizDbContext _dbContext = null!;
    SqliteQuizSnapshotQuestionSnapshotMapRepository _mapRepository = null!;
    SqliteQuizTemplateRepository _templateRepository = null!;
    SqliteQuestionSnapshotRepository _questionSnapshotRepository = null!;
    SqliteQuizSnapshotRepository _quizSnapshotRepository = null!;

    SqliteConnection _connection;

    protected override IQuizSnapshotQuestionSnapshotMapRepository Sut => _mapRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuestionSnapshotRepository QuestionSnapshots => _questionSnapshotRepository;

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
        (_connection, _dbContext) = InitialTestDbHelper.GetTestSqliteQuizDbContext();
        _connection.Open();
        _dbContext.Database.EnsureCreated();

        _mapRepository = new SqliteQuizSnapshotQuestionSnapshotMapRepository(_dbContext);
        _templateRepository = new SqliteQuizTemplateRepository(_dbContext);
        _questionSnapshotRepository = new(_dbContext);
        _quizSnapshotRepository = new(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
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
