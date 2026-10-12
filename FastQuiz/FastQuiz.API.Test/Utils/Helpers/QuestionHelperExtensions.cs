using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Domain.Repositories;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Question.UseCase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.API.Test.Utils.Helpers;

internal static class QuestionHelperExtensions
{
    public static async Task<QuestionGroupDto> CreateQuestionGroupAsync(
        this TestWebAppFactory sut,
        string userId,
        CreateQuestionGroupCommand command
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuestionGroupService>();
        return await service.CreateQuestionGroupAsync(userId, Tenant.DefaultTenantId, command);
    }

    public static async Task<QuestionGroup?> GetQuestionGroupByIdAsync(
        this TestWebAppFactory sut,
        string id
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IQuestionGroupRepository>();
        return await repo.FindByIdAsync(id);
    }

    /// <summary>
    /// 直接经仓储落库一个指定 OwnerId 的题库，用于构造“其他归属”的数据（绕过权限校验）。
    /// </summary>
    public static async Task<QuestionGroup> SeedQuestionGroupWithOwnerAsync(
        this TestWebAppFactory sut,
        string ownerId
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IQuestionGroupRepository>();
        var group = new QuestionGroup(
            Guid.NewGuid().ToString(),
            ownerId,
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        await repo.CreateAsync(group);
        return group;
    }

    public static async Task<QuestionDto> CreateQuestionAsync(
        this TestWebAppFactory sut,
        string userId,
        string groupId,
        CreateQuestionCommand command
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuestionService>();
        return await service.CreateQuestionAsync(userId, Tenant.DefaultTenantId, groupId, command);
    }

    public static async Task<Q?> GetQuestionByIdAsync(this TestWebAppFactory sut, string id)
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IQuestionRepository>();
        return await repo.FindByIdAsync(id);
    }

    public static async Task CleanQuestionSqliteDb(this TestWebAppFactory sut)
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SqliteQuestionDbContext>();
        await db.Answers.ExecuteDeleteAsync();
        await db.Questions.ExecuteDeleteAsync();
        await db.QuestionGroups.ExecuteDeleteAsync();
    }

    /// <summary>
    /// 创建一个随机内容的题库（默认租户、指定操作者）。
    /// </summary>
    public static async Task<QuestionGroupDto> CreateRandomQuestionGroupAsync(
        this TestWebAppFactory sut,
        string userId
    )
    {
        var command = new CreateQuestionGroupCommand(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );
        return await sut.CreateQuestionGroupAsync(userId, command);
    }

    /// <summary>
    /// 构造一个默认的新增题目命令（默认单选、难度 0，含两条答案）。
    /// </summary>
    public static CreateQuestionCommand SeedDefaultCreateQuestionCommand(
        QuestionType type = QuestionType.SingleChoice,
        int level = 0
    )
    {
        return new CreateQuestionCommand(
            Guid.NewGuid().ToString(),
            level,
            type,
            [
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    false,
                    0,
                    MatchStrategy.Manual
                ),
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    true,
                    1,
                    MatchStrategy.Manual
                ),
            ]
        );
    }

    /// <summary>
    /// 构造一个默认的更新题目命令（多选、难度 1，含两条答案）。
    /// </summary>
    public static UpdateQuestionCommand SeedDefaultUpdateQuestionCommand()
    {
        return new UpdateQuestionCommand(
            Guid.NewGuid().ToString(),
            1,
            QuestionType.MultipleChoice,
            [
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    true,
                    0,
                    MatchStrategy.Manual
                ),
                new CreateQuestionAnswerCommand(
                    Guid.NewGuid().ToString(),
                    true,
                    1,
                    MatchStrategy.Manual
                ),
            ]
        );
    }
}
