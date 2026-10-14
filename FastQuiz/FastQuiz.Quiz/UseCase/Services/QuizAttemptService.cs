using EasyCaching.Core;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Domain.Services;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Extensions;
using FastQuiz.Quiz.UseCase.Queries;
using Hangfire;

namespace FastQuiz.Quiz.UseCase.Services;

public class QuizAttemptService(
    IIdGenerator _idGenerator,
    IQuizAttemptRepository _quizAttemptRepository,
    IQuestionRecordRepository _questionRecordRepository,
    IQuizTemplateRepository _quizTemplateRepository,
    IQuizTemplateService _quizTemplateService,
    IQuizSnapshotService _quizSnapshotService,
    IQuestionSnapshotRepository _questionSnapshotRepository,
    IPermissionService _permissionService,
    ITenantService _tenantService,
    IOrganizationService _organizationService,
    IBackgroundJobClient _backgroundJobClient,
    IAnswerBufferService _answerBuffer,
    IQuizReviewService _quizReviewService,
    IEasyCachingProviderFactory _easyCachingProviderFactory
) : IQuizAtteptService
{
    private readonly IEasyCachingProvider _cache =
        _easyCachingProviderFactory.GetQuizTmpCacheProvider();

    #region QuizAttempt
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

    // TODO: 拓展至先找预先生成好的试卷，分配userId
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
                Tenant.DefaultTenantId,
                userId,
                Permissions.SystemAdmin
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
    #endregion

    #region AnswerRecord
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

        var quizAttempt =
            await GetCachedQuizAttemptAsync(questionRecord.QuizAttemptId)
            ?? throw new ResourceNotFoundException();

        var template =
            await GetCachedQuizTemplateAsync(quizAttempt.QuizTemplateId)
            ?? throw new ResourceNotFoundException();

        if (quizAttempt.Status != QuizAttemptStatus.InProgress)
        {
            throw new BusinessException("试卷不在进行中");
        }

        if (quizAttempt.StartTime == null)
        {
            throw new BusinessException("试卷的开始时间异常");
        }

        if (
            DateTimeOffset.UtcNow > quizAttempt.StartTime.Value.AddMinutes(template.DurationMinutes)
        )
        {
            throw new BusinessException("试卷不在可以答题的时间范围内");
        }

        _answerBuffer.SetAnswers(
            new PendingAnswer
            {
                UserId = userId,
                TenantId = tenantId,
                QuestionRecordId = questionRecordId,
                QuizAttemptId = quizAttempt.Id,
                Values = commands.Select(x => x.Value).Distinct().ToArray(),
                AddedTime = DateTimeOffset.UtcNow,
            }
        );
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

    private async Task<QuizAttempt?> GetCachedQuizAttemptAsync(string id)
    {
        var key = $"quiz-attempt:{id}";
        var cached = await _cache.GetAsync<QuizAttempt>(key).ConfigureAwait(false);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var attempt = await _quizAttemptRepository.FindByIdAsync(id).ConfigureAwait(false);

        if (attempt != null)
        {
            // QuizAttempt 状态会变，缓存 5 分钟
            await _cache
                .SetAsync(
                    key,
                    attempt,
                    TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(Random.Shared.Next(-30, 30)))
                )
                .ConfigureAwait(false);
        }

        return attempt;
    }

    private async Task<QuizTemplate?> GetCachedQuizTemplateAsync(string id)
    {
        var key = $"quiz-template:{id}";
        var cached = await _cache.GetAsync<QuizTemplate>(key).ConfigureAwait(false);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var template = await _quizTemplateRepository.FindByIdAsync(id).ConfigureAwait(false);

        if (template != null)
        {
            // 试卷定义不变，缓存 1 小时
            await _cache
                .SetAsync(
                    key,
                    template,
                    TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(Random.Shared.Next(-10, 10)))
                )
                .ConfigureAwait(false);
        }

        return template;
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

    public async Task UpdateQuizAttemptStatusAsync(
        string userId,
        string tenantId,
        string quizAttemptId,
        QuizAttemptStatus status
    )
    {
        var attempt =
            await _quizAttemptRepository.FindByIdAsync(quizAttemptId)
            ?? throw new ResourceNotFoundException();

        if (attempt.AssignId == null)
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

            _backgroundJobClient.Schedule(
                () => ReviewQuizAttemptByAttemptIdAsync(quizAttemptId),
                DateTimeOffset.UtcNow.AddMinutes(template.DurationMinutes)
            );
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
            _backgroundJobClient.Schedule(
                () => ReviewQuizAttemptByAttemptIdAsync(quizAttemptId),
                DateTimeOffset.UtcNow.AddSeconds(5)
            );
            return;
        }

        if (status == QuizAttemptStatus.Graded && attempt.Status == QuizAttemptStatus.Submitted)
        {
            // 收尾阅卷：得分按已定判的每题 IsCorrect × 规则求和(不重判)，并置为 Graded。
            // 适用于全自动判完、或人工逐题(ManualReviewQuestionRecordAsync)判完后收尾。
            await _quizReviewService.CompleteReviewAsync(quizAttemptId);
            return;
        }

        throw new BusinessException($"试卷状态设置异常：{attempt.Status}->{status}");
    }

    public async Task ManualReviewQuestionRecordAsync(
        string tenantId,
        string myId,
        string questionRecordId,
        bool isCorrect
    )
    {
        var record =
            await _questionRecordRepository.FindQuestionRecordByIdAsync(questionRecordId)
            ?? throw new ResourceNotFoundException();

        record.IsCorrect = isCorrect;

        await _questionRecordRepository.UpdateAsync(record);
    }

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

        if (quizAttempt.Status != QuizAttemptStatus.InProgress)
        {
            if (
                !await _permissionService.HasPermissionAsync(
                    tenantId,
                    userId,
                    Permissions.QuizAttempt
                )
            )
            {
                throw new PermissionException();
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

        List<QuestionRecordWithoutAnswerDto> questionRecords = [];

        for (int i = 0; i < questions.Count; i++)
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
                    [
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

        if (quizAttempt.Status != QuizAttemptStatus.Graded)
        {
            if (
                !await _permissionService.HasPermissionAsync(
                    tenantId,
                    userId,
                    Permissions.QuizAttempt
                )
            )
            {
                throw new PermissionException();
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

        for (int i = 0; i < questions.Count; i++)
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
    #endregion
}
