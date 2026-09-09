using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Infrastructure.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.MariaDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FastQuiz.Question.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbQuestionRepositoryTests : QuestionRepositoryTestsBase
{
    private MariaDbQuestionDbContext _dbContext;
    private MariaDbQuestionGroupRepository _questionGroupRepository;
    private MariaDbQuestionRepository _questionRepository;
    protected override IQuestionRepository Sut => _questionRepository;
    protected override IQuestionGroupRepository GroupRepository => _questionGroupRepository;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var connectionString = "Server=127.0.0.1;Database=testdb;User=root;Password=root;";

        var options = new DbContextOptionsBuilder<MariaDbQuestionDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        var settings = Options.Create(new QuestionDbContextSettings());
        _dbContext = new MariaDbQuestionDbContext(options, settings);
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
        var connectionString = "Server=127.0.0.1;Database=testdb;User=root;Password=root;";

        var options = new DbContextOptionsBuilder<MariaDbQuestionDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        var settings = Options.Create(new QuestionDbContextSettings());
        _dbContext = new MariaDbQuestionDbContext(options, settings);

        _questionRepository = new MariaDbQuestionRepository(_dbContext);
        _questionGroupRepository = new MariaDbQuestionGroupRepository(_dbContext);
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
