using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.Infrastructure.Repositories.Sqlite;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;
using FastQuiz.Quiz.UseCase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.API.Test.Utils.Helpers;

internal static class QuizTemplateHelperExtensions
{
    public static async Task<QuizTemplateDto> CreateQuizTemplateAsync(
        this TestWebAppFactory sut,
        string userId,
        CreateQuizTemplateCommand command
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizTemplateService>();
        return await service.CreateQuizTemplateAsync(userId, Tenant.DefaultTenantId, command);
    }

    public static async Task PublishQuizTempalteAsync(
        this TestWebAppFactory sut,
        string userId,
        string templateId,
        bool isPublish
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizTemplateService>();
        await service.UpdateQuizTemplatePublishedAsync(
            userId,
            Tenant.DefaultTenantId,
            templateId,
            new UpdateQuizTemplatePublishedCommand(isPublish)
        );
    }

    public static async Task<QuizTemplate?> GetQuizTemplateByIdAsync(
        this TestWebAppFactory sut,
        string id
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IQuizTemplateRepository>();
        return await repo.FindByIdAsync(id);
    }

    public static async Task<QuestionSelectionRuleDto> CreateQuestionSelectionRuleAsync(
        this TestWebAppFactory sut,
        string userId,
        string templateId,
        CreateQuestionSelectionRuleCommand command
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizTemplateService>();
        return await service.CreateQuestionSelectionRuleAsync(
            userId,
            Tenant.DefaultTenantId,
            templateId,
            command
        );
    }

    public static async Task<QuestionSelectionRule?> GetQuestionSelectionRuleByIdAsync(
        this TestWebAppFactory sut,
        string id
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IQuestionSelectionRuleRepository>();
        return await repo.GetByIdAsync(id);
    }

    public static async Task CleanQuizSqliteDb(this TestWebAppFactory sut)
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SqliteQuizDbContext>();
        await db.QuizTemplates.ExecuteDeleteAsync();
        await db.QuestionSelecionRules.ExecuteDeleteAsync();
        await db.QuizSnapshotQuestionSnapshotMaps.ExecuteDeleteAsync();
        await db.QuestionSnapshots.ExecuteDeleteAsync();
        await db.QuizSnapshots.ExecuteDeleteAsync();
        await db.AnswerRecords.ExecuteDeleteAsync();
        await db.QuestionRecords.ExecuteDeleteAsync();
        await db.QuizAttempts.ExecuteDeleteAsync();
    }

    public static async Task<IReadOnlyList<QuizSnapshotDto>> GetAllQuizSnapshotAsync(
        this TestWebAppFactory sut,
        string userId,
        string templateId,
        GetAllQuizSnapshotQuery query
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizSnapshotService>();
        return await service.GetAllQuizSnapshotAsync(
            userId,
            Tenant.DefaultTenantId,
            templateId,
            query
        );
    }

    public static async Task CreateQuizSnapshotAsync(
        this TestWebAppFactory sut,
        string userId,
        string templateId,
        CreateQuizSnapshotCommand command
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizSnapshotService>();
        await service.CreateQuizSnapshotAsync(userId, Tenant.DefaultTenantId, templateId, command);
    }

    public static async Task<IReadOnlyList<QuizAttempDto>> GetAllQuizAttemptByTemplateIdAsync(
        this TestWebAppFactory sut,
        string userId,
        string templateId,
        GetAllQuizAttemptQuery query
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizAtteptService>();
        return await service.GetAllQuizAttemptAsync(
            userId,
            Tenant.DefaultTenantId,
            templateId,
            query
        );
    }

    public static async Task<QuizAttempDto> GetQuizAttemptByIdAsync(
        this TestWebAppFactory sut,
        string userId,
        string id
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizAtteptService>();
        return await service.GetQuizAttemptByIdAsync(userId, Tenant.DefaultTenantId, id);
    }

    public static async Task<QuizAttempDto> CreateQuizAttemptAsync(
        this TestWebAppFactory sut,
        string userId,
        string templateId
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizAtteptService>();
        return await service.CreateQuizAttemptAsync(userId, Tenant.DefaultTenantId, templateId);
    }

    public static async Task UpdateQuizAttemptStatusAsync(
        this TestWebAppFactory sut,
        string userId,
        string quizAttempt,
        QuizAttemptStatus status
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IQuizAtteptService>();
        await service.UpdateQuizAttemptStatusAsync(
            userId,
            Tenant.DefaultTenantId,
            quizAttempt,
            status
        );
    }
}
