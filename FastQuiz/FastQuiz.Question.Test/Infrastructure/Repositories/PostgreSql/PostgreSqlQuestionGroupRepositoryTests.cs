using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Infrastructure.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Test.Infrastructure.Repositories.PostgreSql;

[TestFixture]
[Category("PostgreSql")]
internal class PostgreSqlQuestionGroupRepositoryTests : QuestionGroupRepositoryTestsBase
{
    private PostgreSqlQuestionDbContext _dbContext;
    private PostgreSqlQuestionGroupRepository _repository;

    protected override IQuestionGroupRepository Sut => _repository;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var connectionString = "Host=127.0.0.1;Database=testdb;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<PostgreSqlQuestionDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var settings = Options.Create(new QuestionDbContextSettings());
        _dbContext = new PostgreSqlQuestionDbContext(options, settings);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.Answers.ExecuteDeleteAsync();
            await _dbContext.Questions.ExecuteDeleteAsync();
            await _dbContext.QuestionGroups.ExecuteDeleteAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        var connectionString = "Host=127.0.0.1;Database=testdb;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<PostgreSqlQuestionDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var settings = Options.Create(new QuestionDbContextSettings());
        _dbContext = new PostgreSqlQuestionDbContext(options, settings);

        _repository = new PostgreSqlQuestionGroupRepository(_dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.Answers.ExecuteDeleteAsync();
            await _dbContext.Questions.ExecuteDeleteAsync();
            await _dbContext.QuestionGroups.ExecuteDeleteAsync();
            await _dbContext.DisposeAsync();
        }
    }

    protected override Task AddQuestionsAsync(IEnumerable<Q> questions)
    {
        _dbContext.Questions.AddRange(questions);
        _dbContext.SaveChanges();
        return Task.CompletedTask;
    }
}
