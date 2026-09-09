using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbQuizSnapshotRepositoryTests : QuizSnapshotRepositoryTestsBase
{
    private MariaDbQuizDbContext _dbContext = null!;
    private MariaDbQuizSnapshotRepository _quizSnapshotRepository = null!;
    private MariaDbQuizTemplateRepository _templateRepository = null!;
    private MariaDbQuizAttemptRepository _attemptRepository = null!;

    protected override IQuizSnapshotRepository Sut => _quizSnapshotRepository;

    protected override IQuizTemplateRepository Templates => _templateRepository;

    protected override IQuizAttemptRepository Attempts => _attemptRepository;

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
        await _dbContext.QuizAttempts.ExecuteDeleteAsync();
        await _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDeleteAsync();
        await _dbContext.QuizSnapshots.ExecuteDeleteAsync();
        await _dbContext.QuizTemplates.ExecuteDeleteAsync();
        await _dbContext.DisposeAsync();
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();

        _quizSnapshotRepository = new MariaDbQuizSnapshotRepository(_dbContext);
        _templateRepository = new MariaDbQuizTemplateRepository(_dbContext);
        _attemptRepository = new MariaDbQuizAttemptRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.QuizAttempts.ExecuteDelete();
        _dbContext.QuizSnapshotQuestionSnapshotMaps.ExecuteDelete();
        _dbContext.QuizSnapshots.ExecuteDelete();
        _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
        _dbContext.QuestionSnapshots.ExecuteDelete();
        _dbContext.QuestionSelecionRules.ExecuteDelete();
        _dbContext.QuizTemplates.ExecuteDelete();
        _dbContext?.Dispose();
    }
}
