using System.Collections.Concurrent;
using EasyCaching.Core;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Common.Validation;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Question.UseCase.Query;
using FastQuiz.Question.UseCase.Services;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Extensions;
using FastQuiz.Quiz.UseCase.Queries;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Services;

public class QuizSnapshotService(
    IQuestionService _questionServie,
    IQuizTemplateService _quizTemplateService,
    IQuizSnapshotRepository _quizSnapshotRepository,
    IQuestionSnapshotRepository _questionSnapshotRepository,
    IQuizSnapshotQuestionSnapshotMapRepository _quizSnapshotQuestionSnapshotMapRepository,
    IQuizUnitOfWork _unitOfWork,
    IIdGenerator _idGenerator,
    IPermissionService _permissionService,
    IEasyCachingProviderFactory _easyCachingProviderFactory,
    IValidator<CreateQuizSnapshotCommand> _createQuizSnapshotCommandValidator,
    IValidator<GetAllQuizSnapshotQuery> _getAllQuizSnapshotQueryValidator
) : IQuizSnapshotService
{
    private readonly IEasyCachingProvider _cache =
        _easyCachingProviderFactory.GetQuestionSnapshotListCacheProvider();

    private static string QuestionSnapshotListCacheKey(string quizSnapshotId) =>
        $"QuestionSnapshotLis@{quizSnapshotId}";

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

    #region QuizSnapshot
    public async Task CreateQuizSnapshotAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        CreateQuizSnapshotCommand command
    )
    {
        _createQuizSnapshotCommandValidator.ValidateAndThrowArgumentException(command);

        if (
            !await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizSnapshot)
        )
        {
            throw new PermissionException();
        }

        var template = await _quizTemplateService
            .GetQuizTemplateAsync(userId, tenantId, quizTemplateId)
            .ConfigureAwait(false);

        if (template.IsPublished)
        {
            throw new QuizSnapshotCreateException("考试模板已发布，不可以创建新的考试快照");
        }

        var ruleList = await _quizTemplateService.GetAllQuestionSelectionRulesAsync(
            userId,
            tenantId,
            quizTemplateId
        );

        for (int i = 0; i < command.Count; i++)
        {
            await CreateQuizSnapshotAsync(userId, tenantId, quizTemplateId, ruleList);
        }
    }

    private async Task CreateQuizSnapshotAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        IReadOnlyList<QuestionSelectionRuleDto> ruleList
    )
    {
        int totalScore = 0;

        foreach (var r in ruleList)
        {
            totalScore += r.Count * r.Score;
        }

        var quizSnapshot = new QuizSnapshot
        {
            Id = _idGenerator.GenerateUlId(),
            QuizTemplateId = quizTemplateId,
            TotalScore = totalScore,
        };

        await _unitOfWork.ExecuteInTransactionAsync(async _ =>
        {
            await _quizSnapshotRepository.CreateAsync(quizSnapshot).ConfigureAwait(false);

            foreach (var rule in ruleList)
            {
                var questions = await _questionServie.GetRandomQuestionAsync(
                    userId,
                    tenantId,
                    rule.GroupId,
                    new GetRandomQuestionQuery(rule.QuestionType, rule.Level, rule.Count)
                );

                if (questions.Count != rule.Count)
                {
                    throw new QuizSnapshotCreateException("试题数量不符合需求，生成试卷已中断");
                }

                var alreadyExistQuestionSnapshots =
                    await _questionSnapshotRepository.FindByQuestionIdAsync(
                        quizTemplateId,
                        questions.Select(x => x.Id)
                    );

                var missingQuestions = questions
                    .Where(q =>
                        !alreadyExistQuestionSnapshots.Select(s => s.QuestionId).Contains(q.Id)
                    )
                    .ToList();

                var newQuestionSnapshots = missingQuestions
                    .Select(x => QuestionDto2QuestionSnapshot(x, quizTemplateId))
                    .ToList();

                await _questionSnapshotRepository
                    .CreateAsync(newQuestionSnapshots)
                    .ConfigureAwait(false);

                var allQuestionSnapshots = alreadyExistQuestionSnapshots
                    .Concat(newQuestionSnapshots)
                    .ToList();

                var allQuestionSnapshotIds = allQuestionSnapshots.Select(x => x.Id).ToList();

                await _quizSnapshotQuestionSnapshotMapRepository.CreateAsync(
                    allQuestionSnapshotIds.Select(x => new QuizSnapshotQuestionSnapshotMap
                    {
                        QuizSnapshotId = quizSnapshot.Id,
                        QuestionSnapshotId = x,
                    })
                ).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    public async Task<QuizSnapshotDto> GetQuizSnapshotByIdAsync(
        string userId,
        string tenantId,
        string quizSnapshotId
    )
    {
        if (
            !await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizSnapshot)
        )
        {
            throw new PermissionException();
        }

        var quizSnapshot =
            await _quizSnapshotRepository.FindByIdAsync(quizSnapshotId).ConfigureAwait(false)
            ?? throw new QuizSnapshotNotFoundException();

        await _quizTemplateService
            .GetQuizTemplateAsync(userId, tenantId, quizSnapshot.QuizTemplateId)
            .ConfigureAwait(false);

        return new QuizSnapshotDto(
            quizSnapshot.Id,
            quizSnapshot.QuizTemplateId,
            quizSnapshot.TotalScore
        );
    }

    public async Task<IReadOnlyList<QuizSnapshotDto>> GetAllQuizSnapshotAsync(
        string userId,
        string tenantId,
        string quiztemplateId,
        GetAllQuizSnapshotQuery query
    )
    {
        _getAllQuizSnapshotQueryValidator.ValidateAndThrowArgumentException(query);

        if (
            !await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizSnapshot)
        )
        {
            throw new PermissionException();
        }

        await _quizTemplateService.GetQuizTemplateAsync(userId, tenantId, quiztemplateId);
        var result = await _quizSnapshotRepository.GetAllAsync(
            quiztemplateId,
            query.Cursor,
            query.Count
        );

        return [.. result.Select(x => new QuizSnapshotDto(x.Id, x.QuizTemplateId, x.TotalScore))];
    }

    public async Task DeleteQuizSnapshotByIdAsync(string userId, string tenantId, string id)
    {
        if (
            !await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizSnapshot)
        )
        {
            throw new PermissionException();
        }

        var quizSnapshot = await _quizSnapshotRepository.FindByIdAsync(id).ConfigureAwait(false);

        if (quizSnapshot == null)
        {
            return;
        }

        var template = await _quizTemplateService
            .GetQuizTemplateAsync(userId, tenantId, quizSnapshot.QuizTemplateId)
            .ConfigureAwait(false);

        if (template.IsPublished)
        {
            throw new QuizSnapshotDeleteException("试卷模板已发布，不可以删除试卷快照");
        }

        await _quizSnapshotRepository.DeleteByIdAsync(id);
    }

    private QuestionSnapshot QuestionDto2QuestionSnapshot(QuestionDto questionDto, string quizTId)
    {
        var questionSnapshotId = _idGenerator.GenerateUlId();
        var answerSnapshots = questionDto
            .Answers.Select(a => new QuestionAnswerSnapshot
            {
                Id = _idGenerator.GenerateUlId(),
                MarkUpContent = a.MarkupContent,
                IsCorrect = a.IsCorrect,
                SortOrder = a.SortOrder,
                QuestionSnapshotId = questionSnapshotId,
                Strategy = a.MatchStrategy,
            })
            .ToList();

        return new QuestionSnapshot
        {
            Id = questionSnapshotId,
            QuizTemplateId = quizTId,
            QuestionId = questionDto.Id,
            MarkUpContent = questionDto.MarkupContent,
            Type = questionDto.Type,
            Level = questionDto.Level,
            GroupId = questionDto.GroupId,
            ParentId = "", //TODO : remove this props
            AnswerSnapshots = answerSnapshots,
        };
    }
    #endregion

    #region QuestionSnapshot

    public async Task<IReadOnlyList<QuestionSnapshotDto>> GetAllQuestionSnapshotAsync(
        string userId,
        string tenantId,
        string quizSnapshotId,
        GetAllQuestionSnapshotQuery query
    )
    {
        if (
            !await _permissionService.HasPermissionAsync(tenantId, userId, Permissions.QuizSnapshot)
        )
        {
            throw new PermissionException();
        }

        var quizSnapshot =
            await _quizSnapshotRepository.FindByIdAsync(quizSnapshotId).ConfigureAwait(false)
            ?? throw new QuizSnapshotNotFoundException();

        await _quizTemplateService
            .GetQuizTemplateAsync(userId, tenantId, quizSnapshot.QuizTemplateId)
            .ConfigureAwait(false);

        var maps = await _quizSnapshotQuestionSnapshotMapRepository
            .GetAllByQuizSnapshotIdAsync(quizSnapshotId, query.Cursor, query.Count)
            .ConfigureAwait(false);

        var questions = await _questionSnapshotRepository
            .FindByIdAsync([.. maps.Select(x => x.QuestionSnapshotId)])
            .ConfigureAwait(false);

        return
        [
            .. questions.Select(q => new QuestionSnapshotDto(
                q.Id,
                q.QuizTemplateId,
                q.QuestionId,
                q.MarkUpContent,
                q.Type,
                q.Level,
                q.GroupId,
                q.ParentId,
                [
                    .. q.AnswerSnapshots.Select(a => new QuestionAnswerSnapshotDto(
                        a.Id,
                        a.MarkUpContent,
                        a.IsCorrect,
                        a.QuestionSnapshotId,
                        a.SortOrder,
                        a.Strategy
                    )),
                ]
            )),
        ];
    }

    public async Task<IReadOnlyList<QuestionSnapshotDto>> GetAllQuestionSnapshotAsync(
        string userId,
        string tenantId,
        string quizSnapshotId
    )
    {
        var quizSnapshot =
            await _quizSnapshotRepository.FindByIdAsync(quizSnapshotId).ConfigureAwait(false)
            ?? throw new QuizSnapshotNotFoundException();

        await _quizTemplateService
            .GetQuizTemplateAsync(userId, tenantId, quizSnapshot.QuizTemplateId)
            .ConfigureAwait(false);

        var cachingKey = QuestionSnapshotListCacheKey(quizSnapshotId);

        var cached = await _cache
            .GetAsync<IReadOnlyList<QuestionSnapshot>>(cachingKey)
            .ConfigureAwait(false);

        IReadOnlyList<QuestionSnapshot> snapshots;

        if (cached.HasValue)
        {
            snapshots = cached.Value;
            return [.. snapshots.Select(ToQuestionSnapshotDto)];
        }

        var lockObj = _cacheLocks.GetOrAdd(cachingKey, new SemaphoreSlim(1, 1));
        await lockObj.WaitAsync().ConfigureAwait(false);

        try
        {
            var doubleCheck = await _cache
                .GetAsync<IReadOnlyList<QuestionSnapshot>>(cachingKey)
                .ConfigureAwait(false);
            if (doubleCheck.HasValue)
            {
                snapshots = doubleCheck.Value;
                return [.. snapshots.Select(ToQuestionSnapshotDto)];
            }

            snapshots = await LoadQuestionSnapshotFromDbAsync(quizSnapshotId).ConfigureAwait(false);

            if (snapshots == null || !snapshots.Any())
            {
                await _cache.SetAsync(
                    cachingKey,
                    Array.Empty<QuestionSnapshot>(),
                    TimeSpan.FromMinutes(5)
                );
                return [];
            }

            var expires = GetRandomExpiration();
            await _cache.SetAsync(cachingKey, snapshots, expires).ConfigureAwait(false);

            return [.. snapshots.Select(ToQuestionSnapshotDto)];
        }
        finally
        {
            lockObj.Release();
        }
    }

    private async Task<IReadOnlyList<QuestionSnapshot>> LoadQuestionSnapshotFromDbAsync(
        string quizSnapshotId
    )
    {
        var maps = await _quizSnapshotQuestionSnapshotMapRepository
            .GetAllByQuizSnapshotIdAsync(quizSnapshotId)
            .ConfigureAwait(false);

        return await _questionSnapshotRepository
            .FindByIdAsync([.. maps.Select(x => x.QuestionSnapshotId)])
            .ConfigureAwait(false);
    }

    private static QuestionSnapshotDto ToQuestionSnapshotDto(QuestionSnapshot q)
    {
        return new QuestionSnapshotDto(
            q.Id,
            q.QuizTemplateId,
            q.QuestionId,
            q.MarkUpContent,
            q.Type,
            q.Level,
            q.GroupId,
            q.ParentId,
            [
                .. q.AnswerSnapshots.Select(a => new QuestionAnswerSnapshotDto(
                    a.Id,
                    a.MarkUpContent,
                    a.IsCorrect,
                    a.QuestionSnapshotId,
                    a.SortOrder,
                    a.Strategy
                )),
            ]
        );
    }

    private static TimeSpan GetRandomExpiration()
    {
        var baseExpiration = TimeSpan.FromHours(6);
        var offsetMinutes = Random.Shared.Next(-60, 60);
        return baseExpiration.Add(TimeSpan.FromMinutes(offsetMinutes));
    }

    public async Task<QuizSnapshotDto> GetQuizSnapshotOfMinAttemptCountByQuizTemplateIdAsync(
        string quizTtemplateId
    )
    {
        var quizSnapshotId = await _quizSnapshotRepository // 直接返回snapshot对象，减少查询次数
            .FindMinAttemptCountAsync(quizTtemplateId)
            .ConfigureAwait(false);

        if (quizSnapshotId == null)
        {
            var result =
                await _quizSnapshotRepository.GetFirstSnapshotByTemplateIdAsync(quizTtemplateId)
                ?? throw new QuizSnapshotNotFoundException();

            return new QuizSnapshotDto(result.Id, result.QuizTemplateId, result.TotalScore);
        }

        var snapshot =
            await _quizSnapshotRepository.FindByIdAsync(quizSnapshotId).ConfigureAwait(false)
            ?? throw new QuizSnapshotNotFoundException();

        return new QuizSnapshotDto(snapshot.Id, snapshot.QuizTemplateId, snapshot.TotalScore);
    }
    #endregion
}
