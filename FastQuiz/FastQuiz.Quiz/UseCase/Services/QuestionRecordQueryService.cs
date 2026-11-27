using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public sealed class QuestionRecordQueryService(
    IQuizAttemptRepository _quizAttemptRepository,
    IQuestionRecordRepository _questionRecordRepository,
    IQuizTemplateService _quizTemplateService,
    IQuestionSnapshotRepository _questionSnapshotRepository,
    IPermissionService _permissionService
) : IQuestionRecordQueryService
{
    public async Task<
        IReadOnlyList<QuestionRecordWithoutAnswerDto>
    > GetAllQuestionWithoutAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    )
    {
        var quizAttempt =
            await _quizAttemptRepository.FindByIdAsync(attemptId)
            ?? throw new ResourceNotFoundException();

        await _quizTemplateService.GetQuizTemplateAsync(userId, tenantId, quizAttempt.QuizTemplateId);

        var canReview = await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt);
        if (quizAttempt.AssignId != userId && !canReview)
        {
            throw new PermissionException();
        }

        if (!canReview && quizAttempt.Status is not (
            QuizAttemptStatus.InProgress or QuizAttemptStatus.Submitted or QuizAttemptStatus.Graded))
        {
            throw new PermissionException();
        }

        var result = await _questionRecordRepository.GetAllByQuizAttempIdAsync(
            attemptId,
            query.Cursor,
            query.Count
        );

        var questions = await _questionSnapshotRepository.FindByIdAsync(
            result.Select(x => x.QuestionSnapshotId)
        );

        List<QuestionRecordWithoutAnswerDto> questionRecords = [];

        for (int i = 0; i < result.Count; i++)
        {
            var record = result[i];
            var q =
                questions.Where(x => x.Id == record.QuestionSnapshotId).FirstOrDefault()
                ?? throw new ResourceNotFoundException("题目快照不存在");

            questionRecords.Add(
                new QuestionRecordWithoutAnswerDto(
                    record.Id,
                    record.QuizAttemptId,
                    record.QuestionSnapshotId,
                    record.MarkUpContent,
                    q.Type,
                    q.Level,
                    q.GroupId,
                    q.ParentId,
                    q.Type == FastQuiz.Question.Domain.Entities.QuestionType.FillIn ? [] : [
                        .. q.AnswerSnapshots.Select(
                            answer => new QuestionAnswerRecordWithoutAnswerDto(
                                answer.Id,
                                answer.MarkUpContent,
                                answer.QuestionSnapshotId,
                                answer.SortOrder,
                                answer.Strategy
                            )
                        ),
                    ],
                    [
                        .. record.AnswerRecords.Select(x => new AnswerRecordDto(
                            x.Id,
                            x.QuizAttemptId,
                            x.QuestionRecordId,
                            x.Value
                        )),
                    ]
                )
            );
        }

        return questionRecords;
    }

    public async Task<IReadOnlyList<QuestionRecordWithAnswerDto>> GetAllQuestionWithAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    )
    {
        var quizAttempt =
            await _quizAttemptRepository.FindByIdAsync(attemptId)
            ?? throw new ResourceNotFoundException();

        var template = await _quizTemplateService.GetQuizTemplateAsync(userId, tenantId, quizAttempt.QuizTemplateId);

        var canReview = await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt);
        if (quizAttempt.AssignId != userId && !canReview)
        {
            throw new PermissionException();
        }

        if (quizAttempt.Status != QuizAttemptStatus.Graded)
        {
            if (!canReview)
            {
                throw new PermissionException();
            }
        }
        else if (!canReview)
        {
            if (!template.IsUnlimitedTime && DateTimeOffset.UtcNow < template.EndTime)
            {
                throw new PermissionException("考试结束前不可查看标准答案");
            }

            if (template.AttemptLimit < 0
                || (template.AttemptLimit > 1
                    && await _quizAttemptRepository.GetAttemptCountAsync(userId, template.Id) < template.AttemptLimit))
            {
                throw new PermissionException("仍有剩余考试次数时不可查看标准答案");
            }
        }

        var result = await _questionRecordRepository.GetAllByQuizAttempIdAsync(
            attemptId,
            query.Cursor,
            query.Count
        );

        var questions = await _questionSnapshotRepository.FindByIdAsync(
            result.Select(x => x.QuestionSnapshotId)
        );

        List<QuestionRecordWithAnswerDto> questionRecords = [];

        for (int i = 0; i < result.Count; i++)
        {
            var record = result[i];
            var q =
                questions.Where(x => x.Id == record.QuestionSnapshotId).FirstOrDefault()
                ?? throw new ResourceNotFoundException("题目快照不存在");

            questionRecords.Add(
                new QuestionRecordWithAnswerDto(
                    record.Id,
                    record.QuizAttemptId,
                    record.QuestionSnapshotId,
                    record.MarkUpContent,
                    q.Type,
                    q.Level,
                    q.GroupId,
                    q.ParentId,
                    [
                        .. q.AnswerSnapshots.Select(answer => new QuestionAnswerSnapshotDto(
                            answer.Id,
                            answer.MarkUpContent,
                            answer.IsCorrect,
                            answer.QuestionSnapshotId,
                            answer.SortOrder,
                            answer.Strategy
                        )),
                    ],
                    [
                        .. record.AnswerRecords.Select(x => new AnswerRecordDto(
                            x.Id,
                            x.QuizAttemptId,
                            x.QuestionRecordId,
                            x.Value
                        )),
                    ],
                    record.IsCorrect
                )
            );
        }

        return questionRecords;
    }
}
