using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Sqlite;

[TestFixture]
[Category("Sqlite")]
internal class SqliteQuizTemplateRepositoryTests : QuizTemplateRepositoryTestsBase
{
    SqliteQuizDbContext _dbContext = null!;
    SqliteQuizTemplateRepository _quizTemplateRepository = null!;
    SqliteConnection _connection;

    protected override IQuizTemplateRepository Sut => _quizTemplateRepository;

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

        _quizTemplateRepository = new SqliteQuizTemplateRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.QuizTemplates.ExecuteDelete();
        _dbContext?.Dispose();
        _connection?.Dispose();
    }
}
