using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions;
using FastQuiz.Common.Utils;
using FastQuiz.Common.Validation;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Exceptions;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Question.UseCase.Query;
using FluentValidation;

namespace FastQuiz.Question.UseCase.Services;

public class QuestionGroupService(
    IQuestionGroupRepository _questionGroupRepository,
    IIdGenerator _idGenerator,
    IPermissionService _permissionService,
    IValidator<CreateQuestionGroupCommand> _createQuestionGroupCommandValidator,
    IValidator<UpdateQuestionGroupCommand> _updateQuestionGroupCommandValidator,
    IValidator<GetAllQuestionGroupQuery> _getAllQuestoinGroupQueryValidator
) : IQuestionGroupService
{
    public async Task<QuestionGroupDto> CreateQuestionGroupAsync(
        string userId,
        string tenantId,
        CreateQuestionGroupCommand command
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        _createQuestionGroupCommandValidator.ValidateAndThrowArgumentException(command);

        var questionGroup = new QuestionGroup(
            _idGenerator.GenerateUlId(),
            tenantId,
            command.Name,
            command.Remark
        );

        await _questionGroupRepository.CreateAsync(questionGroup).ConfigureAwait(false);

        return new QuestionGroupDto(
            questionGroup.Id,
            questionGroup.OwnerId,
            questionGroup.Name,
            questionGroup.Remark,
            0
        );
    }

    public async Task DeleteQuestionGroupByIdAsync(string userId, string tenantId, string groupId)
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        var group = await _questionGroupRepository.FindByIdAsync(groupId).ConfigureAwait(false);

        if (group == null)
        {
            return;
        }

        if (group.OwnerId != tenantId)
        {
            throw new PermissionException();
        }

        await _questionGroupRepository.DeleteAsync(groupId).ConfigureAwait(false);
    }

    public async Task DeleteQuestionGroupByIdAsync(
        string userId,
        string tenantId,
        IReadOnlyList<string> idList
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        var groups = await _questionGroupRepository.FindByIdAsync(idList).ConfigureAwait(false);

        if (groups.Count == 0)
        {
            return;
        }

        if (groups.Any(x => x.OwnerId != tenantId))
        {
            throw new PermissionException();
        }

        await _questionGroupRepository.DeleteAsync(idList).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<QuestionGroupDto>> GetAllQuestionGroupAsync(
        string userId,
        string tenantId,
        GetAllQuestionGroupQuery query
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        _getAllQuestoinGroupQueryValidator.ValidateAndThrowArgumentException(query);

        var result = await _questionGroupRepository
            .GetAllAsync(tenantId, query.Cursor, query.Count, query.IsPrevious, query.NamePrefix)
            .ConfigureAwait(false);

        if (result.Any(x => x.OwnerId != tenantId))
        {
            throw new PermissionException();
        }

        return result;
    }

    public async Task<QuestionGroupDto> GetQuestionGroupByIdAsync(
        string userId,
        string tenantId,
        string groupId
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        var result =
            await _questionGroupRepository.FindDtoByIdAsync(groupId).ConfigureAwait(false)
            ?? throw new QuestionGroupNotFoundException();

        if (result.OwnerId != tenantId)
        {
            throw new PermissionException();
        }

        return result;
    }

    public async Task<int> GetQuestionCountByIdAsync(
        string userId,
        string tenantId,
        string groupId,
        QuestionType type,
        int level
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        var group = await EnsureAndGetQuestionGroupAsync(groupId).ConfigureAwait(false);

        if (group.OwnerId != tenantId)
        {
            throw new PermissionException();
        }

        return await _questionGroupRepository.GetCountByGroupIdAsync(groupId, type, level);
    }

    public async Task UpdateQuestionGroupAsync(
        string userId,
        string tenantId,
        string groupId,
        UpdateQuestionGroupCommand command
    )
    {
        var permission = await _permissionService
            .HasPermissionAsync(tenantId, userId, Permissions.Question)
            .ConfigureAwait(false);

        if (!permission)
        {
            throw new PermissionException();
        }

        _updateQuestionGroupCommandValidator.ValidateAndThrowArgumentException(command);

        var group = await EnsureAndGetQuestionGroupAsync(groupId).ConfigureAwait(false);

        if (group.OwnerId != tenantId)
        {
            throw new PermissionException();
        }

        group.SetName(command.Name);
        group.SetRemark(command.Remark);

        await _questionGroupRepository.UpdateAsync(group).ConfigureAwait(false);
    }

    private async Task<QuestionGroup> EnsureAndGetQuestionGroupAsync(string id)
    {
        return await _questionGroupRepository.FindByIdAsync(id).ConfigureAwait(false)
            ?? throw new QuestionGroupNotFoundException();
    }
}
