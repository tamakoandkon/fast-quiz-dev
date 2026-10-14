using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Domain.Services;
using Hangfire;

namespace FastQuiz.Quiz.UseCase.Services;

public sealed class QuizAttemptLifecycleService(
    IQuizAttemptRepository _quizAttemptRepository,
    IQuizTemplateService _quizTemplateService,
    IPermissionService _permissionService,
    IBackgroundJobClient _backgroundJobClient,
    IQuizReviewService _quizReviewService,
    IQuizUnitOfWork _unitOfWork
) : IQuizAttemptLifecycleService
{
    public async Task UpdateQuizAttemptStatusAsync(
        string userId,
        string tenantId,
        string quizAttemptId,
        QuizAttemptStatus status
    )
    {
        DateTimeOffset? reviewAt = null;
        await _unitOfWork.ExecuteForAttemptAsync(quizAttemptId, async _ =>
        {
            var attempt =
                await _quizAttemptRepository.FindByIdAsync(quizAttemptId)
                ?? throw new ResourceNotFoundException();

            if (attempt.AssignId == null)
            {
                throw new PermissionException();
            }

            if (status == QuizAttemptStatus.Graded)
            {
                if (
                    !await _permissionService
                        .HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt)
                        .ConfigureAwait(false)
                )
                {
                    throw new PermissionException();
                }
            }
            else if (attempt.AssignId != userId)
            {
                throw new PermissionException();
            }

            var template = await _quizTemplateService.GetQuizTemplateAsync(
                userId,
                tenantId,
                attempt.QuizTemplateId
            );

            if (status == attempt.Status)
            {
                return;
            }

            if (status == QuizAttemptStatus.InProgress && attempt.Status == QuizAttemptStatus.Pending)
            {
                attempt.Status = status;
                attempt.StartTime = DateTimeOffset.UtcNow;
                await _quizAttemptRepository.UpdateAsync(attempt);

                reviewAt = attempt.StartTime.Value.AddMinutes(template.DurationMinutes);
                return;
            }

            if (
                status == QuizAttemptStatus.Submitted
                && (
                    attempt.Status == QuizAttemptStatus.Pending
                    || attempt.Status == QuizAttemptStatus.InProgress
                )
            )
            {
                attempt.Status = status;
                attempt.SubmitTime = DateTimeOffset.UtcNow;
                await _quizAttemptRepository.UpdateAsync(attempt);
                reviewAt = DateTimeOffset.UtcNow.AddSeconds(5);
                return;
            }

            if (status == QuizAttemptStatus.Graded && attempt.Status == QuizAttemptStatus.Submitted)
            {
                // 收尾时补齐客观题评分，检查人工题全部判定，再汇总并置为 Graded。
                await _quizReviewService.CompleteReviewAsync(quizAttemptId);
                return;
            }

            throw new BusinessException($"试卷状态设置异常：{attempt.Status}->{status}");
        }).ConfigureAwait(false);

        if (reviewAt.HasValue)
        {
            _backgroundJobClient.Schedule<IQuizAttemptReviewService>(
                service => service.ReviewQuizAttemptByAttemptIdAsync(quizAttemptId), reviewAt.Value);
        }
    }
}
