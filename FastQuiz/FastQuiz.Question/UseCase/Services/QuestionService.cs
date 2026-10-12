using System.Collections.Concurrent;
using EasyCaching.Core;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Common.Validation;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Domain.Servcies;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Question.UseCase.Extensions;
using FastQuiz.Question.UseCase.Query;
using FluentValidation;
using MapsterMapper;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.UseCase.Services;

public class QuestionService(
    IQuestionRepository _questionRepository,
    IQuestionGroupService _questionGroupService,
    IIdGenerator _idGenerator,
    IPermissionService _permissionService,
    IEasyCachingProviderFactory _easyCachingProviderFactory,
    IValidator<CreateQuestionCommand> _createQuestionCommandValidator,
    IValidator<UpdateQuestionCommand> _updateQuestionCommandValidator,
    IValidator<GetAllQuestionQuery> _getAllQuestionQueryQueryValidator,
    IMapper _mapper,
    IQuestionRandomPicker _questionRandomPicker
) : IQuestionService
{
    private readonly IEasyCachingProvider _cache =
        _easyCachingProviderFactory.GetQuestionCacheProvider();

    private static string QuestionIdListCacheKey(string groupId, QuestionType type, int level) =>
        $"QuestionIdList@{groupId}:{type}:{level}";

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _cacheLocks = new();

    public async Task<QuestionDto> CreateQuestionAsync(
        string userId,
        string tenantId,
        string groupId,
        CreateQuestionCommand command
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        _createQuestionCommandValidator.ValidateAndThrowArgumentException(command);
        var group = await _questionGroupService
            .GetQuestionGroupByIdAsync(userId, tenantId, groupId)
            .ConfigureAwait(false);

        var questionId = _idGenerator.GenerateUlId();
        var answers = command
            .Answers.Select(x => new QuestionAnswer(
                _idGenerator.GenerateUlId(),
                questionId,
                x.Content,
                x.IsCorrect,
                x.SortOrder,
                x.MatchStrategy
            ))
            .ToList();
        var question = new Q(
            questionId,
            command.Type,
            command.Level,
            command.Content,
            command.Remark,
            groupId,
            answers
        );

        await _questionRepository.CreateAsync(question).ConfigureAwait(false);

        await _cache
            .RemoveAsync(QuestionIdListCacheKey(groupId, command.Type, command.Level))
            .ConfigureAwait(false);

        return _mapper.Map<QuestionDto>(question);
    }

    public async Task<QuestionDto?> GetQuestionByIdAsync(
        string userId,
        string tenantId,
        string questionId
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        var question = await EnsureAndGetAsync(questionId).ConfigureAwait(false);

        await _questionGroupService.GetQuestionGroupByIdAsync(userId, tenantId, question.GroupId);

        return _mapper.Map<QuestionDto>(question);
    }

    public async Task<IReadOnlyList<QuestionDto>> GetAllQuestionAsync(
        string userId,
        string tenantId,
        string groupId,
        GetAllQuestionQuery query
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        await _questionGroupService
            .GetQuestionGroupByIdAsync(userId, tenantId, groupId)
            .ConfigureAwait(false);

        _getAllQuestionQueryQueryValidator.ValidateAndThrowArgumentException(query);

        var questions = await _questionRepository
            .GetAllAsync(
                groupId,
                query.Type,
                query.Level,
                query.Cursor,
                query.Count,
                query.IsPrevious
            )
            .ConfigureAwait(false);

        return [.. questions.Select(x => _mapper.Map<QuestionDto>(x))];
    }

    public async Task<IReadOnlyList<QuestionDto>> GetRandomQuestionAsync(
        string userId,
        string tenantId,
        string groupId,
        GetRandomQuestionQuery query
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        // Ensure group was exited
        var group = await _questionGroupService
            .GetQuestionGroupByIdAsync(userId, tenantId, groupId)
            .ConfigureAwait(false);

        var result = await _questionRandomPicker
            .PickAsync(groupId, query.Type, query.Level, query.Count)
            .ConfigureAwait(false);

        return [.. result.Select(x => _mapper.Map<QuestionDto>(x))];
    }

    public async Task UpdateQuestionAsync(
        string userId,
        string tenantId,
        string questionId,
        UpdateQuestionCommand command
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        _updateQuestionCommandValidator.ValidateAndThrowArgumentException(command);

        var result =
            await _questionRepository.FindByIdAsync(questionId).ConfigureAwait(false)
            ?? throw new QuestionNotFoundException();
        var group = await _questionGroupService
            .GetQuestionGroupByIdAsync(userId, tenantId, result.GroupId)
            .ConfigureAwait(false);

        result.SetMarkupContent(command.Content);
        result.SetLevel(command.Level);
        result.SetType(command.Type);
        result.SetRemark(command.Remark);

        var answers = command
            .Answers.Select(x => new QuestionAnswer(
                _idGenerator.GenerateUlId(),
                questionId,
                x.Content,
                x.IsCorrect,
                x.SortOrder,
                x.MatchStrategy
            ))
            .ToList();
        result.SetAnswers(answers);

        await _questionRepository.UpdateAsync(result).ConfigureAwait(false);
        await _cache
            .RemoveAsync(QuestionIdListCacheKey(group.Id, command.Type, command.Level))
            .ConfigureAwait(false);
    }

    public async Task DeleteByIdAsync(string userId, string tenantId, string questionId)
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        var question = await _questionRepository.FindByIdAsync(questionId).ConfigureAwait(false);

        if (question == null)
        {
            return;
        }

        await _questionGroupService
            .GetQuestionGroupByIdAsync(userId, tenantId, question.GroupId)
            .ConfigureAwait(false);

        await _questionRepository.DeleteAsync(questionId).ConfigureAwait(false);
        await _cache
            .RemoveAsync(QuestionIdListCacheKey(question.GroupId, question.Type, question.Level))
            .ConfigureAwait(false);
    }

    private async Task<Q> EnsureAndGetAsync(string id)
    {
        return await _questionRepository.FindByIdAsync(id).ConfigureAwait(false)
            ?? throw new QuestionNotFoundException();
    }
}
