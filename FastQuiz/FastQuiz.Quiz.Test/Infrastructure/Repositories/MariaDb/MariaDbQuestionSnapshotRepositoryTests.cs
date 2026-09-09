using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.MariaDb;
using FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;
using Microsoft.EntityFrameworkCore;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.MariaDb;

[TestFixture]
[Category("MariaDb")]
internal class MariaDbQuestionSnapshotRepositoryTests : QuestionSnapshotRepositoryTestsBase
{
    private MariaDbQuizDbContext _dbContext = null!;
    private MariaDbQuestionSnapshotRepository _questionSnapshotRepository = null!;
    private MariaDbQuizTemplateRepository _templateRepository = null!;

    protected override IQuestionSnapshotRepository Sut => _questionSnapshotRepository;

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
            await _dbContext.QuestionAnswerSnapshots.ExecuteDeleteAsync();
            await _dbContext.QuestionSnapshots.ExecuteDeleteAsync();
            await _dbContext.QuizTemplates.ExecuteDeleteAsync();
            await _dbContext.DisposeAsync();
        }
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = InitialTestDbHelper.GetTestMariaDbQuizDbContext();

        _questionSnapshotRepository = new MariaDbQuestionSnapshotRepository(_dbContext);
        _templateRepository = new MariaDbQuizTemplateRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        if (_dbContext != null)
        {
            _dbContext.QuestionAnswerSnapshots.ExecuteDelete();
            _dbContext.QuestionSnapshots.ExecuteDelete();
            _dbContext.QuestionSelecionRules.ExecuteDelete();
            _dbContext.QuizTemplates.ExecuteDelete();
            _dbContext?.Dispose();
        }
    }

    #region Utils

    protected override async Task<List<QuestionAnswerSnapshot>> GetQuestionAnswerSnapshotsAsync(
        string questionId
    )
    {
        return await _dbContext
            .QuestionAnswerSnapshots.AsNoTracking()
            .Where(x => x.QuestionSnapshotId == questionId)
            .ToListAsync();
    }
    #endregion
}
