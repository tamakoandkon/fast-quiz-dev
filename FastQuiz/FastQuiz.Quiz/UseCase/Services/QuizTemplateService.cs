using System.Data;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Common.Validation;
using FastQuiz.Question.UseCase.Services;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Services;

public class QuizTemplateService(
    IQuizTemplateRepository _quizTemplateRepository,
    IQuestionSelectionRuleRepository _questionSelectionRuleRepository,
    IQuestionGroupService _questionGroupService,
    IIdGenerator _idGenerator,
    IPermissionService _permissinService,
    IQuizSnapshotRepository _quizSnapshotRepository,
    IOrganizationService _organizationService,
    ITenantService _tenantService,
    IValidator<CreateQuizTemplateCommand> _createQuizTemplateCommandValidator,
    IValidator<UpdateQuizTemplateCommand> _updateQuizTemplateCommandValidator,
    IValidator<CreateQuestionSelectionRuleCommand> _createQuestionSelectionRuleCommandValidator,
    IValidator<UpdateQuestionSelectionRuleCommand> _updateQuestionSelectionRuleCommandValidator,
    IValidator<GetAllQuestionSelectionRuleQuery> _getAllQuestionSelectionRuleQueryValidator,
    IValidator<GetAllQuizTemplateQuery> _getAllQuizTemplateQueryValidator,
    IValidator<GetPublishedQuizTemplateQuery> _getPublishedQuizTemplateQueryValidator,
    IValidator<UpdateQuizTemplatePublishedCommand> _updateQuizTemplatePublishedCommandValidator
) : IQuizTemplateService
{
    #region QuizTemplate

    public async Task<QuizTemplateDto> CreateQuizTemplateAsync(
        string userId,
        string tenantId,
        CreateQuizTemplateCommand command
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        _createQuizTemplateCommandValidator.ValidateAndThrowArgumentException(command);

        if (
            !IsValidDuration(
                command.IsUnlimitedTime,
                command.BeginTime,
                command.EndTime,
                command.DurationMinutes
            )
        )
        {
            throw new QuizTemplateCreateException("Invalid duration time");
        }

        if (command.UserGroupId != null)
        {
            // Ensure organization is existed
            await _organizationService
                .GetByIdAsync(userId, command.UserGroupId)
                .ConfigureAwait(false);
        }

        var template = new QuizTemplate(
            _idGenerator.GenerateUlId(),
            command.Name,
            command.Description,
            command.Remark,
            tenantId,
            command.AttemptLimit,
            command.IsUnlimitedTime,
            command.BeginTime,
            command.EndTime,
            command.DurationMinutes,
            command.UserGroupId
        );

        await _quizTemplateRepository.CreateAsync(template).ConfigureAwait(false);

        return new QuizTemplateDto(
            template.Id,
            template.Name,
            template.Description,
            template.Remark,
            template.AttemptLimit,
            template.IsUnlimitedTime,
            template.BeginTime,
            template.EndTime,
            template.DurationMinutes,
            template.IsPublished,
            template.OwnerId,
            template.CreatedAt,
            template.UpdatedAt,
            template.UserGroupId
        );
    }

    public async Task<QuizTemplateDto> GetQuizTemplateAsync(
        string userId,
        string tenantId,
        string templateId
    )
    {
        var isInTenant = await _tenantService
            .GetUserInTenantAsync(tenantId, userId)
            .ConfigureAwait(false);

        if (!isInTenant)
        {
            throw new PermissionException();
        }

        var template = await EnsureAndGetQuizTemplateAsync(templateId).ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        return new QuizTemplateDto(
            template.Id,
            template.Name,
            template.Description,
            template.Remark,
            template.AttemptLimit,
            template.IsUnlimitedTime,
            template.BeginTime,
            template.EndTime,
            template.DurationMinutes,
            template.IsPublished,
            template.OwnerId,
            template.CreatedAt,
            template.UpdatedAt,
            template.UserGroupId
        );
    }

    public async Task<IReadOnlyList<QuizTemplateDto>> GetAllQuizTemplateAsync(
        string userId,
        string tenantId,
        GetAllQuizTemplateQuery query
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        _getAllQuizTemplateQueryValidator.ValidateAndThrowArgumentException(query);

        var result = await _quizTemplateRepository
            .GetAllAsync(tenantId, query.Cursor, query.Count, query.IsPrevious, null, null)
            .ConfigureAwait(false);

        return
        [
            .. result.Select(x => new QuizTemplateDto(
                x.Id,
                x.Name,
                x.Description,
                x.Remark,
                x.AttemptLimit,
                x.IsUnlimitedTime,
                x.BeginTime,
                x.EndTime,
                x.DurationMinutes,
                x.IsPublished,
                x.OwnerId,
                x.CreatedAt,
                x.UpdatedAt,
                x.UserGroupId
            )),
        ];
    }

    public async Task<IReadOnlyList<QuizTemplateDto>> GetPublishedQuizTemplateAsync(
        string userId,
        string tenantId,
        GetPublishedQuizTemplateQuery query
    )
    {
        if (!await _tenantService.GetUserInTenantAsync(tenantId, userId))
            throw new PermissionException();
        _getPublishedQuizTemplateQueryValidator.ValidateAndThrowArgumentException(query);

        var result = await _quizTemplateRepository
            .GetAllAsync(tenantId, query.Cursor, query.Count, false, null, true)
            .ConfigureAwait(false);

        return
        [
            .. result.Select(x => new QuizTemplateDto(
                x.Id,
                x.Name,
                x.Description,
                x.Remark,
                x.AttemptLimit,
                x.IsUnlimitedTime,
                x.BeginTime,
                x.EndTime,
                x.DurationMinutes,
                x.IsPublished,
                x.OwnerId,
                x.CreatedAt,
                x.UpdatedAt,
                x.UserGroupId
            )),
        ];
    }

    public async Task UpdateQuizTemplateAsync(
        string userId,
        string tenantId,
        string templateId,
        UpdateQuizTemplateCommand command
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        _updateQuizTemplateCommandValidator.ValidateAndThrowArgumentException(command);

        var template = await EnsureAndGetQuizTemplateAsync(templateId).ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        if (template.IsPublished)
        {
            throw new QuizTemplateUpdateException("已发布的试卷模板不可修改");
        }

        if (
            !IsValidDuration(
                command.IsUnlimitedTime,
                command.BeginTime,
                command.EndTime,
                command.DurationMinutes
            )
        )
        {
            throw new QuizTemplateUpdateException("修改后的时间不合法");
        }

        template.Name = command.Name;
        template.Description = command.Description;
        template.Remark = command.Remark;
        template.AttemptLimit = command.AttemptLimit;
        template.BeginTime = command.BeginTime;
        template.EndTime = command.EndTime;
        template.DurationMinutes = command.DurationMinutes;
        template.UserGroupId = command.UserGroupId;
        template.UpdatedAt = DateTimeOffset.UtcNow;

        await _quizTemplateRepository.UpdateAsync(template);
    }

    public async Task UpdateQuizTemplatePublishedAsync(
        string userId,
        string tenantId,
        string templateId,
        UpdateQuizTemplatePublishedCommand command
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        _updateQuizTemplatePublishedCommandValidator.ValidateAndThrowArgumentException(command);

        var template = await EnsureAndGetQuizTemplateAsync(templateId).ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        if (template.IsPublished == command.IsPublished)
        {
            return;
        }

        if (command.IsPublished == true)
        {
            if (!template.IsUnlimitedTime && template.BeginTime < DateTimeOffset.UtcNow)
            {
                throw new QuizTemplateUpdateException("当前时间已超过开始时间");
            }

            var rules = await _questionSelectionRuleRepository
                .GetByQuizTemplateIdAsync(template.Id, null, 1)
                .ConfigureAwait(false);
            if (rules.Count == 0)
            {
                throw new QuizTemplateUpdateException("至少需要有一个抽题规则");
            }

            var snapshots = await _quizSnapshotRepository
                .GetAllAsync(templateId, null, 1)
                .ConfigureAwait(false);

            if (snapshots.Count == 0)
            {
                throw new QuizTemplateUpdateException("至少需要有一个试卷快照");
            }
        }

        template.IsPublished = command.IsPublished;
        template.UpdatedAt = DateTimeOffset.UtcNow;
        await _quizTemplateRepository.UpdateAsync(template).ConfigureAwait(false);
    }

    public async Task DeleteQuizTemplateAsync(string userId, string tenantId, string templateId)
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
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

        var template = await _quizTemplateRepository.FindByIdAsync(templateId);

        if (template is null)
        {
            return;
        }

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("Can not get quiz template of other.");
        }

        if (template.IsPublished)
        {
            throw new QuizTemplateUpdateException("已发布的试卷模板不可修改");
        }

        await _quizTemplateRepository.DeleteByIdAsync(templateId).ConfigureAwait(false);
    }

    #endregion

    #region QuestionSelectionRule
    public async Task<QuestionSelectionRuleDto> CreateQuestionSelectionRuleAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        CreateQuestionSelectionRuleCommand command
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        _createQuestionSelectionRuleCommandValidator.ValidateAndThrowArgumentException(command);

        var template = await EnsureAndGetQuizTemplateAsync(quizTemplateId).ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        if (template.IsPublished)
        {
            throw new QuizTemplateUpdateException("已发布的试卷模板不可修改");
        }

        await _questionGroupService
            .GetQuestionGroupByIdAsync(userId, tenantId, command.GroupId)
            .ConfigureAwait(false);

        var exitedRule = await _questionSelectionRuleRepository
            .GetByTemplateIdAndConditionAsync(
                template.Id,
                command.GroupId,
                command.QuestionType,
                command.Level
            )
            .ConfigureAwait(false);

        if (exitedRule != null)
        {
            throw new QuestionSelectionRuleCreateException("同类型的抽题规则已经存在");
        }

        var rule = new QuestionSelectionRule
        {
            Id = _idGenerator.GenerateUlId(),
            QuizeTemplateId = quizTemplateId,
            Count = command.Count,
            GroupId = command.GroupId,
            Level = command.Level,
            Score = command.Score,
            Type = command.QuestionType,
        };

        await _questionSelectionRuleRepository.CreateAsync(rule).ConfigureAwait(false);

        await DeleteObseletedQuizSnapshotAsync(template.Id);

        return new QuestionSelectionRuleDto(
            rule.Id,
            rule.QuizeTemplateId,
            rule.GroupId,
            rule.Type,
            rule.Level,
            rule.Count,
            rule.Score
        );
    }

    public async Task<QuestionSelectionRuleDto> GetQuestionSelectionRuleByIdAsync(
        string userId,
        string tenantId,
        string questionSelectionId
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        var result =
            await _questionSelectionRuleRepository
                .GetByIdAsync(questionSelectionId)
                .ConfigureAwait(false) ?? throw new QuestionSelectionRuleNotFoundException();

        var template = await EnsureAndGetQuizTemplateAsync(result.QuizeTemplateId)
            .ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        return new QuestionSelectionRuleDto(
            result.Id,
            result.QuizeTemplateId,
            result.GroupId,
            result.Type,
            result.Level,
            result.Count,
            result.Score
        );
    }

    public async Task<IReadOnlyList<QuestionSelectionRuleDto>> GetAllQuestionSelectionRulesAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllQuestionSelectionRuleQuery query
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        _getAllQuestionSelectionRuleQueryValidator.ValidateAndThrowArgumentException(query);
        var template = await EnsureAndGetQuizTemplateAsync(quizTemplateId).ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        var result = await _questionSelectionRuleRepository
            .GetByQuizTemplateIdAsync(quizTemplateId, query.Cursor, query.Count)
            .ConfigureAwait(false);

        return
        [
            .. result.Select(rule => new QuestionSelectionRuleDto(
                rule.Id,
                rule.QuizeTemplateId,
                rule.GroupId,
                rule.Type,
                rule.Level,
                rule.Count,
                rule.Score
            )),
        ];
    }

    public async Task<IReadOnlyList<QuestionSelectionRuleDto>> GetAllQuestionSelectionRulesAsync(
        string userId,
        string tenantId,
        string quizTemplateId
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        var template = await EnsureAndGetQuizTemplateAsync(quizTemplateId).ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        var result = await _questionSelectionRuleRepository
            .GetByQuizTemplateIdAsync(quizTemplateId)
            .ConfigureAwait(false);

        return
        [
            .. result.Select(rule => new QuestionSelectionRuleDto(
                rule.Id,
                rule.QuizeTemplateId,
                rule.GroupId,
                rule.Type,
                rule.Level,
                rule.Count,
                rule.Score
            )),
        ];
    }

    public async Task UpdateQuestionSelectionRuleAsync(
        string userId,
        string tenantId,
        string ruleId,
        UpdateQuestionSelectionRuleCommand command
    )
    {
        _updateQuestionSelectionRuleCommandValidator.ValidateAndThrowArgumentException(command);

        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        var rule = await EnsureAndGetQuestionSelectionRuleAsync(ruleId).ConfigureAwait(false);

        var template = await EnsureAndGetQuizTemplateAsync(rule.QuizeTemplateId)
            .ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        if (template.IsPublished)
        {
            throw new QuizTemplateUpdateException("已发布的试卷模板不可修改");
        }

        var exitedRule = await _questionSelectionRuleRepository
            .GetByTemplateIdAndConditionAsync(
                template.Id,
                command.GroupId,
                command.QuestionType,
                command.Level
            )
            .ConfigureAwait(false);

        if (exitedRule != null && exitedRule.Id != ruleId)
        {
            throw new QuestionSelectionRuleCreateException("同类型的抽题规则已经存在");
        }

        rule.GroupId = command.GroupId;
        rule.Type = command.QuestionType;
        rule.Level = command.Level;
        rule.Score = command.Score;
        rule.Count = command.Count;

        await _questionSelectionRuleRepository.UpdateAsync(rule).ConfigureAwait(false);
        await DeleteObseletedQuizSnapshotAsync(template.Id);
    }

    public async Task DeleteQuestionSelectionRuleByIdAsync(
        string userId,
        string tenantId,
        string id
    )
    {
        if (!await _permissinService.HasPermissionAsync(tenantId, userId, Permissions.QuizTemplate))
        {
            throw new PermissionException();
        }

        var rule = await _questionSelectionRuleRepository.GetByIdAsync(id);

        if (rule == null)
        {
            return;
        }

        var template = await EnsureAndGetQuizTemplateAsync(rule.QuizeTemplateId)
            .ConfigureAwait(false);

        if (template.OwnerId != tenantId)
        {
            throw new PermissionException("无法查看不属于当前租户的试卷模板");
        }

        if (template.IsPublished)
        {
            throw new QuizTemplateUpdateException("已发布的试卷模板不可修改");
        }

        await _questionSelectionRuleRepository.DeleteByIdAsync(id);
        await DeleteObseletedQuizSnapshotAsync(template.Id);
    }

    #endregion

    private async Task<QuizTemplate> EnsureAndGetQuizTemplateAsync(string id) =>
        await _quizTemplateRepository.FindByIdAsync(id).ConfigureAwait(false)
        ?? throw new QuizTemplateNotFoundException("Quiz template is not found.");

    private async Task<QuestionSelectionRule> EnsureAndGetQuestionSelectionRuleAsync(string id) =>
        await _questionSelectionRuleRepository.GetByIdAsync(id).ConfigureAwait(false)
        ?? throw new QuestionSelectionRuleNotFoundException(
            "Question selection rule is not found."
        );

    /// <summary>
    /// 在抽题规则有变动的时候删除过时的试卷快照
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    private async Task DeleteObseletedQuizSnapshotAsync(string templateId)
    {
        await _quizSnapshotRepository.DeleteByQuizTemplateIdAsync(templateId).ConfigureAwait(false);
    }

    private static bool IsValidDuration(
        bool IsUnlimitedTime,
        DateTimeOffset BeginTime,
        DateTimeOffset EndTime,
        int DurationMinutes
    )
    {
        if (IsUnlimitedTime)
        {
            return true;
        }

        if (DurationMinutes <= 0)
        {
            return true;
        }

        TimeSpan actualDuration = EndTime - BeginTime;
        return actualDuration >= TimeSpan.FromMinutes(DurationMinutes);
    }
}
