using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.PostgreSQL;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.PostgreSql;

[TestFixture]
[Category("PostgreSql")]
internal class PostgreSQLQuestionSelectionRuleRepositoryTests
    : QuestionSelectionRuleRepositoryTestsBase
{
    private PostgreSQLQuizDbContext _dbContext = null!;
    private PostgreSQLQuestionSelectionRuleRepository _ruleRepository = null!;
    private PostgreSQLQuizTemplateRepository _quizTemplateRepository = null!;

    protected override IQuestionSelectionRuleRepository Sut => _ruleRepository;

    protected override IQuizTemplateRepository TemplateRepository => _quizTemplateRepository;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_dbContext != null)
        {
            await ClearAllTablesAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestPostgreSqlQuizDbContext();

        _ruleRepository = new PostgreSQLQuestionSelectionRuleRepository(_dbContext);
        _quizTemplateRepository = new PostgreSQLQuizTemplateRepository(_dbContext);
    }

    [TearDown]
    public async Task TearDown()
    {
        await ClearAllTablesAsync();
        await _dbContext.DisposeAsync();
    }

    private async Task ClearAllTablesAsync()
    {
        await _dbContext.AnswerRecords.ExecuteDeleteAsync();
        await _dbContext.QuestionRecords.ExecuteDeleteAsync();
        await _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDeleteAsync();
        await _dbContext.QuestionAnswerSnapshots.ExecuteDeleteAsync();
        await _dbContext.QuestionSnapshots.ExecuteDeleteAsync();
        await _dbContext.QuizAttempts.ExecuteDeleteAsync();
        await _dbContext.QuizSnapshots.ExecuteDeleteAsync();
        await _dbContext.QuestionSelecionRules.ExecuteDeleteAsync();
        await _dbContext.QuizTemplates.ExecuteDeleteAsync();
    }
}
