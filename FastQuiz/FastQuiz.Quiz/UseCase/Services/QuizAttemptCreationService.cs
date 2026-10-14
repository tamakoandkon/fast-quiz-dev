using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;

namespace FastQuiz.Quiz.UseCase.Services;

public sealed class QuizAttemptCreationService(
    IIdGenerator _idGenerator,
    IQuizAttemptRepository _quizAttemptRepository,
    IQuestionRecordRepository _questionRecordRepository,
    IQuizTemplateService _quizTemplateService,
    IQuizSnapshotService _quizSnapshotService,
    IPermissionService _permissionService,
    ITenantService _tenantService,
    IOrganizationService _organizationService
) : IQuizAttemptCreationService
{
    public async Task<QuizAttempDto> CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId
    )
    {
        // 用户是否属于这个租户
        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        // 用户是否存在没有结束的考试
        var existedAttempt = await _quizAttemptRepository
            .FindLastUnfinishedByTemplateIdAndUserIdAsync(userId, quizTemplateId)
            .ConfigureAwait(false);

        if (existedAttempt is not null)
        {
            throw new QuizAttemptCreateException("还有未结束的考试");
        }

        // 考试模板是否存在
        var template = await _quizTemplateService
            .GetQuizTemplateAsync(userId, tenantId, quizTemplateId)
            .ConfigureAwait(false);

        // 用户是否属于设定的用户组
        if (template.UserGroupId != null)
        {
            var isInOrg = await _organizationService
                .GetUserInOrganizationAsync(template.UserGroupId, userId)
                .ConfigureAwait(false);
            if (!isInOrg)
            {
                throw new PermissionException();
            }
        }

        // 考试是否发布
        if (!template.IsPublished)
        {
            throw new QuizAttemptCreateException("无法参加未发布的考试");
        }

        // 是否在可以参加考试的时间范围
        var currentTime = DateTimeOffset.UtcNow;

        if (
            !template.IsUnlimitedTime
            && (template.BeginTime > currentTime || template.EndTime < currentTime)
        )
        {
            throw new QuizAttemptCreateException("考试已结束");
        }

        var attemptCount = await _quizAttemptRepository
            .GetAttemptCountAsync(userId, template.Id)
            .ConfigureAwait(false);
        if ((attemptCount + 1) > template.AttemptLimit)
        {
            throw new QuizAttemptCreateException($"考试次数已经达到上限");
        }

        return await CreateQuizAttemptAsync(userId, tenantId, template.Id, template.Name)
            .ConfigureAwait(false);
    }

    public async Task CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        IEnumerable<CreateQuizAttemptCommand> commands
    )
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt))
        {
            throw new PermissionException();
        }

        var template = await _quizTemplateService.GetQuizTemplateAsync(
            userId,
            tenantId,
            quizTemplateId
        );
        foreach (var command in commands)
        {
            var quizSnaptshot = await _quizSnapshotService.GetQuizSnapshotByIdAsync(
                userId,
                tenantId,
                command.QuizSnapshotId
            );

            var attempt = new QuizAttempt()
            {
                Id = _idGenerator.GenerateUlId(),
                AssignId = null,
                QuizSnapshotId = quizSnaptshot.Id,
                QuizTemplateId = template.Id,
                Status = QuizAttemptStatus.Pending,
                Score = 0,
                StartTime = null,
                SubmitTime = null,
            };

            await _quizAttemptRepository.CreateAsync(attempt);

            var questionSnapshots = await _quizSnapshotService.GetAllQuestionSnapshotAsync(
                userId,
                tenantId,
                quizSnaptshot.Id
            );

            var questionRecords = questionSnapshots.Select(x => new QuestionRecord
            {
                Id = _idGenerator.GenerateUlId(),
                QuizAttemptId = attempt.Id,
                MarkUpContent = x.MarkUpContent,
                QuestionSnapshotId = x.Id,
            });

            await _questionRecordRepository.CreateAsync(questionRecords);
        }
    }

    private async Task<QuizAttempDto> CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string templateId,
        string templateName
    )
    {
        var snapshot = await _quizSnapshotService
            .GetQuizSnapshotOfMinAttemptCountByQuizTemplateIdAsync(templateId)
            .ConfigureAwait(false);

        var attempt = new QuizAttempt()
        {
            Id = _idGenerator.GenerateUlId(),
            AssignId = userId,
            QuizSnapshotId = snapshot.Id,
            QuizTemplateId = templateId,
            Status = QuizAttemptStatus.Pending,
            Score = 0,
            StartTime = null,
            SubmitTime = null,
        };

        await _quizAttemptRepository.CreateAsync(attempt).ConfigureAwait(false);

        var questionSnapshots = await _quizSnapshotService
            .GetAllQuestionSnapshotAsync(userId, tenantId, snapshot.Id)
            .ConfigureAwait(false);

        var questionRecords = questionSnapshots.Select(x => new QuestionRecord
        {
            Id = _idGenerator.GenerateUlId(),
            QuizAttemptId = attempt.Id,
            MarkUpContent = x.MarkUpContent,
            QuestionSnapshotId = x.Id,
        });

        await _questionRecordRepository.CreateAsync(questionRecords).ConfigureAwait(false);

        return new QuizAttempDto(
            attempt.Id,
            attempt.AssignId,
            attempt.QuizTemplateId,
            templateName,
            attempt.QuizSnapshotId,
            attempt.StartTime,
            attempt.SubmitTime,
            attempt.Score,
            attempt.Status
        );
    }

    public async Task AssignQuizAttemptAsync(string userId, string tenantId, string quizAttemptId)
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt))
        {
            throw new PermissionException();
        }

        var attempt =
            await _quizAttemptRepository.FindByIdAsync(quizAttemptId)
            ?? throw new ResourceNotFoundException();

        var template = await _quizTemplateService.GetQuizTemplateAsync(
            userId,
            tenantId,
            attempt.QuizTemplateId
        );

        if (!template.IsPublished)
        {
            throw new BusinessException("无法参加未发布的考试");
        }

        var currentTime = DateTimeOffset.UtcNow;

        if (
            !template.IsUnlimitedTime
            && (template.BeginTime > currentTime || template.EndTime < currentTime)
        )
        {
            throw new BusinessException();
        }

        var attemptCount = await _quizAttemptRepository.GetAttemptCountAsync(
            userId,
            attempt.QuizTemplateId
        );
        if ((attemptCount + 1) > template.AttemptLimit)
        {
            throw new BusinessException($"Attempt limit {attemptCount + 1}");
        }

        if (attempt.AssignId != null)
        {
            throw new PermissionException();
        }

        attempt.AssignId = userId;
        await _quizAttemptRepository.UpdateAsync(attempt);
    }
}
