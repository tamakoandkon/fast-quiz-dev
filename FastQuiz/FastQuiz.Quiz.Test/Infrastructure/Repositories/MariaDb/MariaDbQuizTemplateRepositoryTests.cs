using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbQuizTemplateRepositoryTests : QuizTemplateRepositoryTestsBase
{
    private MariaDbQuizDbContext _dbContext = null!;
    private MariaDbQuizTemplateRepository _quizTemplateRepository = null!;

    protected override IQuizTemplateRepository Sut => _quizTemplateRepository;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            await _dbContext.QuizTemplates.ExecuteDeleteAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();

        _quizTemplateRepository = new MariaDbQuizTemplateRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        if (_dbContext != null)
        {
            _dbContext.QuizTemplates.ExecuteDelete();
            _dbContext.Dispose();
        }
    }
}
