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
    /// <summary>
    /// 新增一个题库
    /// </summary>
    /// <param name="userId">操作者id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="command">新增题库命令</param>
    /// <returns>新增后的题库实体</returns>
    /// <exception cref="PermissionException">当
    /// 当用户没有创建该题库的权限时抛出。详见 <see cref="PermissionException"/>。
    /// </exception>
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

    /// <summary>
    /// 删除单个题库
    /// </summary>
    /// <param name="userId">操作者id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="groupId">题库id</param>
    /// <returns></returns>
    /// <exception cref="PermissionException">
    /// 当用户没有删除该题库的权限时抛出。详见 <see cref="PermissionException"/>。
    /// </exception>
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

    /// <summary>
    ///删除多个题库
    /// </summary>
    /// <param name="userId">操作者id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="idList">题库id列表</param>
    /// <returns></returns>
    /// <exception cref="PermissionException">当用户没有删除该题库的权限时抛出。详见 <see cref="PermissionException"/>。</exception>
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

    /// <summary>
    /// 分页获取所有题库
    /// </summary>
    /// <param name="userId">操作者id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="query">查询条件</param>
    /// <returns></returns>
    /// <exception cref="PermissionException">当用户没有访问该题库的权限时抛出。详见 <see cref="PermissionException"/>。</exception>
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

    /// <summary>
    /// 根据题库id获取题库信息
    /// </summary>
    /// <param name="userId">操作者id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="groupId">题库id</param>
    /// <returns></returns>
    /// <exception cref="PermissionException">当用户没有访问该题库的权限时抛出。详见 <see cref="PermissionException"/>。</exception>
    /// <exception cref="QuestionGroupNotFoundException">当指定的题库不存在时抛出。详见 <see cref="QuestionGroupNotFoundException"/>。</exception>
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

    /// <summary>
    /// 根据题库id获取题目数量
    /// </summary>
    /// <param name="userId">操作者id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="groupId">题库id</param>
    /// <param name="type">题目类型</param>
    /// <param name="level">题目难度</param>
    /// <returns></returns>
    /// <exception cref="PermissionException">当用户没有访问该题库的权限时抛出。详见 <see cref="PermissionException"/>。</exception>
    public async Task<int> GetQuestionCountByIdAsync(
        string userId,
        string tenantId,
        string groupId,
        GetQuestionCountQuery query
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

        return await _questionGroupRepository
            .GetCountByGroupIdAsync(groupId, query.Type, query.Level)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 根据题库id获取题目统计信息
    /// </summary>
    /// <param name="userId">操作员id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="groupId">题库id</param>
    /// <returns>题库统计信息</returns>
    /// <exception cref="PermissionException">当用户没有访问该题库的权限时抛出。详见 <see cref="PermissionException"/>。</exception>
    public async Task<QuestionGroupStatisticsDto> GetQuestionGroupStatisticsAsync(
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

        var group = await EnsureAndGetQuestionGroupAsync(groupId).ConfigureAwait(false);

        if (group.OwnerId != tenantId)
        {
            throw new PermissionException();
        }

        return await _questionGroupRepository
            .GetStatisticsByGroupIdAsync(groupId)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// 更新题库信息
    /// </summary>
    /// <param name="userId">操作员id</param>
    /// <param name="tenantId">所属租户id</param>
    /// <param name="groupId">题库id</param>
    /// <param name="command">更新命令</param>
    /// <returns></returns>
    /// <exception cref="PermissionException">当用户没有访问该题库的权限时抛出。详见 <see cref="PermissionException"/>。</exception>
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
