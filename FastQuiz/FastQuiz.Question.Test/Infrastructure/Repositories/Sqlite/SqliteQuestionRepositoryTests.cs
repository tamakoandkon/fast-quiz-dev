using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.Test.Infrastructure.Repositories.Sqlite;

[TestFixture]
[Category("Sqlite")]
internal class SqliteQuestionRepositoryTests : QuestionRepositoryTestsBase
{
    private SqliteQuestionDbContext _questionDbContext;
    private SqliteQuestionRepository _questionRepository;
    private SqliteQuestionGroupRepository _groupRepository;

    protected override IQuestionRepository Sut => _questionRepository;
    protected override IQuestionGroupRepository GroupRepository => _groupRepository;

    private SqliteConnection _connection;

    [SetUp]
    public void Setup()
    {
        var connectionString = "Data Source=:memory:";

        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        var options = new DbContextOptionsBuilder<SqliteQuestionDbContext>()
            .UseSqlite(_connection)
            .Options;

        _questionDbContext = new SqliteQuestionDbContext(
            options,
            Options.Create(new Question.Infrastructure.Repositories.QuestionDbContextSettings())
        );
        _questionDbContext.Database.EnsureCreated();

        _questionRepository = new SqliteQuestionRepository(_questionDbContext);
        _groupRepository = new SqliteQuestionGroupRepository(_questionDbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _questionDbContext?.Dispose();
        _connection?.Dispose();
    }
}
