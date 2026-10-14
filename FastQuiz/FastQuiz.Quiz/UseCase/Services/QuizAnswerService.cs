using EasyCaching.Core;
using FastQuiz.Common.Exceptions;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Extensions;

namespace FastQuiz.Quiz.UseCase.Services;

public sealed class QuizAnswerService(
    IQuizAttemptRepository _quizAttemptRepository,
    IQuestionRecordRepository _questionRecordRepository,
    IQuizTemplateService _quizTemplateService,
    IAnswerBufferService _answerBuffer,
    IQuizUnitOfWork _unitOfWork,
    IEasyCachingProviderFactory _easyCachingProviderFactory
) : IQuizAnswerService
{
    private readonly IEasyCachingProvider _cache =
        _easyCachingProviderFactory.GetQuizTmpCacheProvider();

    public async Task AnswerQuestionAsync(
        string userId,
        string tenantId,
        string questionRecordId,
        IEnumerable<SetAnswerRecordCommand> commands
    )
    {
        var questionRecord =
            await GetCachedQuestionRecordAsync(questionRecordId)
            ?? throw new ResourceNotFoundException();

        await _unitOfWork.ExecuteForAttemptAsync(questionRecord.QuizAttemptId, async _ =>
        {
            // State and ownership are checked after acquiring the same lock used by submission.
            var quizAttempt = await _quizAttemptRepository.FindByIdAsync(questionRecord.QuizAttemptId)
                ?? throw new ResourceNotFoundException();
            if (quizAttempt.AssignId != userId)
            {
                throw new PermissionException();
            }

            var template = await _quizTemplateService.GetQuizTemplateAsync(
                userId, tenantId, quizAttempt.QuizTemplateId);
            if (quizAttempt.Status != QuizAttemptStatus.InProgress)
            {
                throw new BusinessException("试卷不在进行中");
            }
            if (quizAttempt.StartTime == null)
            {
                throw new BusinessException("试卷的开始时间异常");
            }
            if (DateTimeOffset.UtcNow >= quizAttempt.StartTime.Value.AddMinutes(template.DurationMinutes))
            {
                throw new BusinessException("试卷不在可以答题的时间范围内");
            }

            await _answerBuffer.SetAnswersAsync(new PendingAnswer
            {
                UserId = userId,
                TenantId = tenantId,
                QuestionRecordId = questionRecordId,
                QuizAttemptId = quizAttempt.Id,
                Values = commands.Select(x => x.Value).Distinct().ToArray(),
                AddedTime = DateTimeOffset.UtcNow,
            }).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task<QuestionRecord?> GetCachedQuestionRecordAsync(string id)
    {
        var key = $"question-record:{id}";
        var cached = await _cache.GetAsync<QuestionRecord>(key).ConfigureAwait(false);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var record = await _questionRecordRepository
            .FindQuestionRecordByIdAsync(id)
            .ConfigureAwait(false);

        if (record != null)
        {
            await _cache
                .SetAsync(
                    key,
                    record,
                    TimeSpan.FromMinutes(30).Add(TimeSpan.FromSeconds(Random.Shared.Next(-60, 60)))
                )
                .ConfigureAwait(false);
        }

        return record;
    }
}
