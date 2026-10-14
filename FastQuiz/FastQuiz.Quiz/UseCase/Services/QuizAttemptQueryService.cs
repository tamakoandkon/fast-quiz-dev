using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public sealed class QuizAttemptQueryService(
    IQuizAttemptRepository _quizAttemptRepository,
    IQuizTemplateRepository _quizTemplateRepository,
    IQuizTemplateService _quizTemplateService,
    IPermissionService _permissionService,
    ITenantService _tenantService
) : IQuizAttemptQueryService
{
    public async Task<IReadOnlyList<QuizAttempDto>> GetAllQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllQuizAttemptQuery query
    )
    {
        if (!await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt))
        {
            throw new PermissionException();
        }

        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        var attempts = await _quizAttemptRepository.GetAllByTemplateIdAsync(
            quizTemplateId,
            query.Cursor,
            query.Count
        );

        var templates = await _quizTemplateRepository.FindByIdAsync(
            [.. attempts.Select(x => x.QuizTemplateId)]
        );

        return attempts
            .Where(x => x.AssignId != null)
            .Select(quizAttempt => new QuizAttempDto(
                quizAttempt.Id,
                quizAttempt.AssignId!,
                quizAttempt.QuizTemplateId,
                templates.Where(x => x.Id == quizAttempt.QuizTemplateId).First().Name,
                quizAttempt.QuizSnapshotId,
                quizAttempt.StartTime,
                quizAttempt.SubmitTime,
                quizAttempt.Score,
                quizAttempt.Status
            ))
            .ToList();
    }

    public async Task<IReadOnlyList<QuizAttempDto>> GetAllRecordsByTemplateIdAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllRecordsQuery query
    )
    {
        // 该操作属于 quiz 模块的后台管理能力，仅允许拥有 quiz 模块权限(QuizAttempt)的用户使用。
        if (!await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizAttempt))
        {
            throw new PermissionException();
        }

        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        var attempts = await _quizAttemptRepository
            .GetAllRecordsByTemplateIdAsync(
                quizTemplateId,
                query.OnlyBest,
                query.Cursor,
                query.Count,
                query.IsPrevious
            )
            .ConfigureAwait(false);

        var templates = await _quizTemplateRepository.FindByIdAsync(
            [.. attempts.Select(x => x.QuizTemplateId)]
        );

        return
        [
            .. attempts
                .Where(x => x.AssignId != null)
                .Select(x => new QuizAttempDto(
                    x.Id,
                    x.AssignId!,
                    x.QuizTemplateId,
                    templates.Where(t => t.Id == x.QuizTemplateId).First().Name,
                    x.QuizSnapshotId,
                    x.StartTime,
                    x.SubmitTime,
                    x.Score,
                    x.Status
                )),
        ];
    }

    public async Task<IReadOnlyList<QuizAttempDto>> GetMyQuizAttemptAsync(
        string userId,
        string tenantId,
        GetMyQuizAttemptQuery query
    )
    {
        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        var attempts = await _quizAttemptRepository
            .GetAllByUserIdAsync(userId, query.Cursor, query.Count)
            .ConfigureAwait(false);

        var templates = await _quizTemplateRepository.FindByIdAsync(
            [.. attempts.Select(x => x.QuizTemplateId)]
        );

        return
        [
            .. attempts.Select(x => new QuizAttempDto(
                x.Id,
                x.AssignId!,
                x.QuizTemplateId,
                templates.Where(t => t.Id == x.QuizTemplateId).First().Name,
                x.QuizSnapshotId,
                x.StartTime,
                x.SubmitTime,
                x.Score,
                x.Status
            )),
        ];
    }

    public async Task<IReadOnlyList<QuizAttempDto>> GetMyTemplateRecordsAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetMyTemplateRecordsQuery query
    )
    {
        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        // 只看自己的记录，故无需 quiz 模块权限校验(与 GetUserRecordsAsync 的「本人」分支同义)。
        var attempts = await _quizAttemptRepository
            .GetAllRecordsByTemplateIdAndUserIdAsync(
                userId,
                quizTemplateId,
                query.Cursor,
                query.Count,
                query.IsPrevious
            )
            .ConfigureAwait(false);

        var templates = await _quizTemplateRepository.FindByIdAsync(
            [.. attempts.Select(x => x.QuizTemplateId)]
        );

        return
        [
            .. attempts.Select(x => new QuizAttempDto(
                x.Id,
                x.AssignId!,
                x.QuizTemplateId,
                templates.FirstOrDefault(t => t.Id == x.QuizTemplateId)?.Name
                    ?? x.QuizTemplateId,
                x.QuizSnapshotId,
                x.StartTime,
                x.SubmitTime,
                x.Score,
                x.Status
            )),
        ];
    }

    public async Task<IReadOnlyList<QuizAttempDto>> GetUserRecordsAsync(
        string operatorId,
        string tenantId,
        string userId,
        GetUserRecordsQuery query
    )
    {
        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, operatorId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        // 查看自己的考试信息始终允许；查看他人的则需要 quiz 模块权限(QuizAttempt)。
        if (
            operatorId != userId
            && !await _permissionService.HasPermissionAsync(
                tenantId,
                operatorId,
                Permissions.QuizAttempt
            )
        )
        {
            throw new PermissionException();
        }

        var attempts = await _quizAttemptRepository
            .GetAllRecordsByUserIdAsync(
                userId,
                query.TemplateId,
                query.OnlyBest,
                query.Cursor,
                query.Count,
                query.IsPrevious
            )
            .ConfigureAwait(false);

        var templates = await _quizTemplateRepository.FindByIdAsync(
            [.. attempts.Select(x => x.QuizTemplateId)]
        );

        return
        [
            .. attempts.Select(x => new QuizAttempDto(
                x.Id,
                x.AssignId!,
                x.QuizTemplateId,
                templates.Where(t => t.Id == x.QuizTemplateId).First().Name,
                x.QuizSnapshotId,
                x.StartTime,
                x.SubmitTime,
                x.Score,
                x.Status
            )),
        ];
    }

    public async Task<QuizAttempDto> GetQuizAttemptByIdAsync(
        string userId,
        string tenantId,
        string quizAttemptId
    )
    {
        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        var quizAttempt =
            await _quizAttemptRepository.FindByIdAsync(quizAttemptId).ConfigureAwait(false)
            ?? throw new ResourceNotFoundException();

        if (
            userId != quizAttempt.AssignId
            && !await _permissionService.HasPermissionAsync(
                tenantId,
                userId,
                Permissions.QuizAttempt
            )
        )
        {
            throw new PermissionException();
        }

        var template = await _quizTemplateService.GetQuizTemplateAsync(
            userId,
            tenantId,
            quizAttempt.QuizTemplateId
        );

        return new QuizAttempDto(
            quizAttempt.Id,
            quizAttempt.AssignId!,
            quizAttempt.QuizTemplateId,
            template.Name,
            quizAttempt.QuizSnapshotId,
            quizAttempt.StartTime,
            quizAttempt.SubmitTime,
            quizAttempt.Score,
            quizAttempt.Status
        );
    }
}
