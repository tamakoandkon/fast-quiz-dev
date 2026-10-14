using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Domain.Services;

namespace FastQuiz.Quiz.UseCase.Services;

public sealed class QuizAttemptReviewService(
    IQuizAttemptRepository _quizAttemptRepository,
    IQuestionRecordRepository _questionRecordRepository,
    IQuizTemplateService _quizTemplateService,
    IQuestionSnapshotRepository _questionSnapshotRepository,
    IPermissionService _permissionService,
    IQuizReviewService _quizReviewService,
    IQuizUnitOfWork _unitOfWork
) : IQuizAttemptReviewService
{
    public async Task ManualReviewQuestionRecordAsync(
        string tenantId,
        string myId,
        string questionRecordId,
        bool isCorrect
    )
    {
        if (
            !await _permissionService
                .HasPermissionAsync(tenantId, myId, Permissions.QuizAttempt)
                .ConfigureAwait(false)
        )
        {
            throw new PermissionException();
        }

        var record =
            await _questionRecordRepository.FindQuestionRecordByIdAsync(questionRecordId)
            ?? throw new ResourceNotFoundException();

        await _unitOfWork.ExecuteForAttemptAsync(record.QuizAttemptId, async _ =>
        {
            var attempt =
                await _quizAttemptRepository.FindByIdAsync(record.QuizAttemptId)
                ?? throw new ResourceNotFoundException();

            await _quizTemplateService
                .GetQuizTemplateAsync(myId, tenantId, attempt.QuizTemplateId)
                .ConfigureAwait(false);

            if (attempt.Status != QuizAttemptStatus.Submitted)
            {
                throw new BusinessException("只有待阅卷的试卷可以人工判题");
            }

            var question =
                await _questionSnapshotRepository.FindByIdAsync(record.QuestionSnapshotId)
                ?? throw new ResourceNotFoundException("题目快照不存在");

            if (question.Type != QuestionType.FillIn
                || !question.AnswerSnapshots.Any(x => x.Strategy == MatchStrategy.Manual))
            {
                throw new BusinessException("该题不需要人工判题");
            }

            record.IsCorrect = isCorrect;

            await _questionRecordRepository.UpdateAsync(record);
        }).ConfigureAwait(false);
    }

    public async Task ReviewQuizAttemptByAttemptIdAsync(string quizAttemptId)
    {
        // 自动阅卷逻辑收敛到领域服务；此处仅保持原方法签名(被后台调度/事件引用)。
        await _quizReviewService.ReviewAttemptAsync(quizAttemptId);
    }

    public async Task ReviewQuizAttemptsByTemplateIdAsync(string templateId)
    {
        // step1
        var attempts = await _quizAttemptRepository.GetAllByTemplateIdAsync(templateId, null, 200);

        // step2
        foreach (var attempt in attempts)
        {
            await ReviewQuizAttemptByAttemptIdAsync(attempt.Id);
        }
    }
}
