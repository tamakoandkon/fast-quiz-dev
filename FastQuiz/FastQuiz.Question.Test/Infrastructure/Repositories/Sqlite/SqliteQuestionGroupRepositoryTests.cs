using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Test.Infrastructure.Repositories.Sqlite;

[TestFixture]
[Category("Sqlite")]
public class SqliteQuestionGroupRepositoryTests : QuestionGroupRepositoryTestsBase
{
    private SqliteQuestionDbContext _dbContext;
    private SqliteQuestionGroupRepository _repository;
    private SqliteConnection _connection;

    protected override IQuestionGroupRepository Sut => _repository;

    [SetUp]
    public void Setup()
    {
        var connectionString = "Data Source=:memory:";

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        var options = new DbContextOptionsBuilder<SqliteQuestionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new SqliteQuestionDbContext(
            options,
            Options.Create(new Question.Infrastructure.Repositories.QuestionDbContextSettings())
        );
        _dbContext.Database.EnsureCreated();

        _repository = new SqliteQuestionGroupRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
        _connection?.Dispose();
    }

    protected override async Task AddQuestionsAsync(IEnumerable<Q> questions)
    {
        await _dbContext.Questions.AddRangeAsync(questions);
        await _dbContext.SaveChangesAsync();
    }
}
