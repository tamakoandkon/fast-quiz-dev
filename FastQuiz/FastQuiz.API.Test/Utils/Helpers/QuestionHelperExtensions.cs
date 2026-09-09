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
}
