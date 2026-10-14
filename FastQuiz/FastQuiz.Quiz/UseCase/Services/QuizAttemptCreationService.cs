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
    IIdGenerator idGenerator,
    IQuizAttemptRepository attempts,
    IQuestionRecordRepository records,
    IQuizTemplateService templates,
    IQuizSnapshotService snapshots,
    IPermissionService permissions,
    ITenantService tenants,
    IOrganizationService organizations,
    IQuizUnitOfWork unitOfWork
) : IQuizAttemptCreationService
{
    public async Task<QuizAttempDto> CreateQuizAttemptAsync(
        string userId, string tenantId, string quizTemplateId)
    {
        // Validate tenant ownership before taking the database lock.
        await templates.GetQuizTemplateAsync(userId, tenantId, quizTemplateId);
        QuizAttempDto? result = null;
        await unitOfWork.ExecuteForTemplateAsync(quizTemplateId, async _ =>
        {
            var template = await EnsureEligibleAsync(userId, tenantId, quizTemplateId);
            var snapshot = await snapshots.GetQuizSnapshotOfMinAttemptCountByQuizTemplateIdAsync(quizTemplateId);
            var attempt = await CreateFromSnapshotAsync(userId, tenantId, template.Id, snapshot.Id, userId);
            result = ToDto(attempt, template.Name);
        });
        return result!;
    }

    public async Task CreateQuizAttemptAsync(
        string userId, string tenantId, string quizTemplateId,
        IEnumerable<CreateQuizAttemptCommand> commands)
    {
        await EnsureManagerAsync(userId, tenantId, quizTemplateId);
        var batch = commands?.ToList();
        if (batch is null || batch.Count == 0 || batch.Count > 100
            || batch.Any(x => x is null || string.IsNullOrWhiteSpace(x.QuizSnapshotId)
                || x.Count is < 1 or > 100)
            || batch.Sum(x => (long)x.Count) > 100)
            throw new BusinessException("每次批量生成须为 1–100 份完整试卷");

        // Every command and every question record belongs to this single transaction.
        await unitOfWork.ExecuteForTemplateAsync(quizTemplateId, async _ =>
        {
            foreach (var command in batch)
            {
                var snapshot = await snapshots.GetQuizSnapshotByIdAsync(userId, tenantId, command.QuizSnapshotId);
                if (snapshot.QuizTemplateId != quizTemplateId)
                    throw new PermissionException("不能使用其他考试模板的快照");
                for (var i = 0; i < command.Count; i++)
                    await CreateFromSnapshotAsync(userId, tenantId, quizTemplateId, snapshot.Id, null);
            }
        });
    }

    public async Task<IReadOnlyList<PrebuiltQuizAttemptDto>> GetPrebuiltQuizAttemptsAsync(
        string userId, string tenantId, string templateId, string? cursor, int count)
    {
        await EnsureManagerAsync(userId, tenantId, templateId);
        if (count is < 1 or > 100)
            throw new BusinessException("每页数量须在 1–100 之间");
        var pool = await attempts.GetUnassignedByTemplateIdAsync(templateId, cursor, count);
        return pool.Select(x => new PrebuiltQuizAttemptDto(x.Id, x.QuizSnapshotId)).ToList();
    }

    public async Task<QuizAttempDto> ClaimAvailableQuizAttemptAsync(
        string userId, string tenantId, string templateId)
    {
        await templates.GetQuizTemplateAsync(userId, tenantId, templateId);
        QuizAttempDto? result = null;
        await unitOfWork.ExecuteForTemplateAsync(templateId, async _ =>
        {
            var template = await EnsureEligibleAsync(userId, tenantId, templateId);
            var pool = await attempts.GetUnassignedByTemplateIdAsync(templateId, null, 1);
            var attempt = pool.FirstOrDefault()
                ?? throw new BusinessException("暂无可领取的预生成试卷，请联系管理员");
            await BindAsync(attempt, userId);
            result = ToDto(attempt, template.Name);
        });
        return result!;
    }

    public async Task AssignQuizAttemptAsync(string userId, string tenantId, string quizAttemptId)
    {
        if (!await tenants.GetUserInTenantAsync(tenantId, userId))
            throw new PermissionException();
        var candidate = await attempts.FindByIdAsync(quizAttemptId)
            ?? throw new ResourceNotFoundException("试卷不存在");
        await templates.GetQuizTemplateAsync(userId, tenantId, candidate.QuizTemplateId);
        await unitOfWork.ExecuteForTemplateAsync(candidate.QuizTemplateId, async _ =>
        {
            await EnsureEligibleAsync(userId, tenantId, candidate.QuizTemplateId);
            // Read again after the lock; stale candidates must never overwrite a winner.
            var attempt = await attempts.FindByIdAsync(quizAttemptId)
                ?? throw new ResourceNotFoundException("试卷不存在");
            await BindAsync(attempt, userId);
        });
    }

    private async Task BindAsync(QuizAttempt attempt, string userId)
    {
        if (attempt.AssignId is not null || attempt.Status != QuizAttemptStatus.Pending)
            throw new BusinessException("该试卷已被领取或不可领取");
        var questions = await records.GetAllByQuizAttempIdAsync(attempt.Id);
        if (questions.Count == 0)
            throw new BusinessException("试卷缺少题目，请联系管理员");
        attempt.AssignId = userId;
        await attempts.UpdateAsync(attempt);
    }

    private async Task<QuizTemplateDto> EnsureEligibleAsync(
        string userId, string tenantId, string templateId)
    {
        var template = await templates.GetQuizTemplateAsync(userId, tenantId, templateId);
        if (!template.IsPublished)
            throw new QuizAttemptCreateException("无法参加未发布的考试");
        var now = DateTimeOffset.UtcNow;
        if (!template.IsUnlimitedTime && (template.BeginTime > now || template.EndTime < now))
            throw new QuizAttemptCreateException("考试尚未开始或已结束");
        if (template.UserGroupId is not null
            && !await organizations.GetUserInOrganizationAsync(template.UserGroupId, userId))
            throw new PermissionException("不属于本场考试的指定组织");
        if (await attempts.FindLastUnfinishedByTemplateIdAndUserIdAsync(userId, templateId) is not null)
            throw new QuizAttemptCreateException("还有未结束的考试，请从我的考试继续");
        var count = await attempts.GetAttemptCountAsync(userId, templateId);
        if (template.AttemptLimit >= 0 && count >= template.AttemptLimit)
            throw new QuizAttemptCreateException("考试次数已经达到上限");
        return template;
    }

    private async Task EnsureManagerAsync(string userId, string tenantId, string templateId)
    {
        if (!await permissions.HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt))
            throw new PermissionException();
        await templates.GetQuizTemplateAsync(userId, tenantId, templateId);
    }

    private async Task<QuizAttempt> CreateFromSnapshotAsync(
        string operatorId, string tenantId, string templateId, string snapshotId, string? assignee)
    {
        var questions = await snapshots.GetAllQuestionSnapshotAsync(operatorId, tenantId, snapshotId);
        if (questions.Count == 0)
            throw new BusinessException("不能生成没有题目的试卷");
        var attempt = new QuizAttempt
        {
            Id = idGenerator.GenerateUlId(), AssignId = assignee,
            QuizSnapshotId = snapshotId, QuizTemplateId = templateId,
            Status = QuizAttemptStatus.Pending, Score = 0
        };
        await attempts.CreateAsync(attempt);
        await records.CreateAsync(questions.Select(x => new QuestionRecord
        {
            Id = idGenerator.GenerateUlId(), QuizAttemptId = attempt.Id,
            MarkUpContent = x.MarkUpContent, QuestionSnapshotId = x.Id
        }));
        return attempt;
    }

    private static QuizAttempDto ToDto(QuizAttempt attempt, string templateName) => new(
        attempt.Id, attempt.AssignId!, attempt.QuizTemplateId, templateName,
        attempt.QuizSnapshotId, attempt.StartTime, attempt.SubmitTime, attempt.Score, attempt.Status);
}
