using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbQuestionSelectionRuleRepositoryTests
    : QuestionSelectionRuleRepositoryTestsBase
{
    private MariaDbQuizDbContext _dbContext = null!;
    private MariaDbQuestionSelectionRuleRepository _ruleRepository = null!;
    private MariaDbQuizTemplateRepository _templateRepository = null!;

    protected override IQuestionSelectionRuleRepository Sut => _ruleRepository;

    protected override IQuizTemplateRepository TemplateRepository => _templateRepository;

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
            await _dbContext.QuestionSelecionRules.ExecuteDeleteAsync();
            await _dbContext.QuizTemplates.ExecuteDeleteAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();

        _ruleRepository = new MariaDbQuestionSelectionRuleRepository(_dbContext);
        _templateRepository = new MariaDbQuizTemplateRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.QuestionSelecionRules.ExecuteDelete();
        _dbContext.QuizTemplates.ExecuteDelete();
        _dbContext.Dispose();
    }
}
