using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Infrastructure.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.Test.Infrastructure.Repositories.PostgreSql;

internal class PostgreSqlQuestionRepositoryTests : QuestionRepositoryTestsBase
{
    private PostgreSqlQuestionDbContext _dbContext;
    private PostgreSqlQuestionGroupRepository _questionGroupRepository;
    private PostgreSqlQuestionRepository _questionRepository;
    protected override IQuestionRepository Sut => _questionRepository;
    protected override IQuestionGroupRepository GroupRepository => _questionGroupRepository;

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

        _questionGroupRepository = new PostgreSqlQuestionGroupRepository(_dbContext);
        _questionRepository = new PostgreSqlQuestionRepository(_dbContext);
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
}
