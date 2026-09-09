using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Sqlite;

internal class SqliteQuestionSnapshotRepositoryTests : QuestionSnapshotRepositoryTestsBase
{
    SqliteQuizDbContext _dbContext = null!;
    SqliteQuestionSnapshotRepository _questionSnapshotRepository = null!;
    SqliteQuizTemplateRepository _templateRepository = null!;

    SqliteConnection _connection;

    protected override IQuestionSnapshotRepository Sut => _questionSnapshotRepository;

    protected override IQuizTemplateRepository TemplateRepository => _templateRepository;

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

        _questionSnapshotRepository = new SqliteQuestionSnapshotRepository(_dbContext);
        _templateRepository = new SqliteQuizTemplateRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
        _dbContext.QuestionSnapshots.ExecuteDelete();
        _dbContext.QuestionSelecionRules.ExecuteDelete();
        _dbContext.QuizTemplates.ExecuteDelete();
        _dbContext?.Dispose();
        _connection?.Dispose();
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
}
