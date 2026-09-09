using FastQuiz.Common.Exceptions.Extensions;
using FastQuiz.Common.Utils;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;
using FastQuiz.Quiz.UseCase.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FastQuiz.Quiz.Interface;

public static class QuizTemplateApi
{
    public static void MapQuizTemplateApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenant/{tenantId}/quiz-template")
            .RequireAuthorization()
            .WithTags("QuizTemplate")
            .WithStandardErrorMetadata();

        #region QuizTemplate
        group.MapPost("", PostQuizTemplateAsync).Produces<QuizTemplateDto>(StatusCodes.Status200OK);

        group
            .MapGet("", GetAllQuizTemplateAsync)
            .Produces<List<QuizTemplateDto>>(StatusCodes.Status200OK);

        group
            .MapGet("published", GetPublishedQuizTemplateAsync)
            .Produces<List<QuizTemplateDto>>(StatusCodes.Status200OK);

        group
            .MapGet("{quizTemplateId}", GetQuizTemplateByIdAsync)
            .Produces<QuizTemplateDto>(StatusCodes.Status200OK);
        group
            .MapPut("{quizTemplateId}", PutQuizTemplateByIdAsync)
            .Produces(StatusCodes.Status204NoContent);
        group
            .MapPut("{quizTemplateId}/published", PutQuizTemplatePublishedByIdAsync)
            .Produces(StatusCodes.Status204NoContent);

        group
            .MapDelete("{quizTemplateId}", DeleteQuizTemplateByIdAsync)
            .Produces(StatusCodes.Status204NoContent);
        #endregion

        #region QuestionSelectionRule

        group
            .MapPost("{quizTemplateId}/rule", PostQuestionSelectionRuleAsync)
            .Produces<QuestionSelectionRuleDto>(StatusCodes.Status200OK);

        group
            .MapGet("{quizTemplateId}/rule", GetAllQuestionSelectionRuleAsync)
            .Produces<List<QuestionSelectionRuleDto>>(StatusCodes.Status200OK);

        group
            .MapGet("rule/{ruleId}", GetQuestionSelectionRuleByIdAsync)
            .Produces<QuestionSelectionRuleDto>(StatusCodes.Status200OK);

        group
            .MapPut("rule/{ruleId}", PutQuestionSelectionRuleAsync)
            .Produces(StatusCodes.Status204NoContent);

        group
            .MapDelete("rule/{ruleId}", DeleteQuestionSelectionRuleAsync)
            .Produces(StatusCodes.Status204NoContent);

        #endregion
    }

    private static async Task<IResult> PostQuizTemplateAsync(
        string tenantId,
        CreateQuizTemplateCommand command,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.CreateQuizTemplateAsync(userId, tenantId, command));
    }

    private static async Task<IResult> GetAllQuizTemplateAsync(
        string tenantId,
        [AsParameters] GetAllQuizTemplateQuery query,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetAllQuizTemplateAsync(userId, tenantId, query));
    }

    private static async Task<IResult> GetPublishedQuizTemplateAsync(
        string tenantId,
        [AsParameters] GetPublishedQuizTemplateQuery query,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetPublishedQuizTemplateAsync(userId, tenantId, query));
    }

    private static async Task<IResult> GetQuizTemplateByIdAsync(
        string tenantId,
        string quizTemplateId,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();
        return Results.Ok(await service.GetQuizTemplateAsync(userId, tenantId, quizTemplateId));
    }

    private static async Task<IResult> PutQuizTemplateByIdAsync(
        string tenantId,
        string quizTemplateId,
        UpdateQuizTemplateCommand command,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.UpdateQuizTemplateAsync(userId, tenantId, quizTemplateId, command);

        return Results.NoContent();
    }

    private static async Task<IResult> PutQuizTemplatePublishedByIdAsync(
        string tenantId,
        string quizTemplateId,
        UpdateQuizTemplatePublishedCommand command,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.UpdateQuizTemplatePublishedAsync(userId, tenantId, quizTemplateId, command);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteQuizTemplateByIdAsync(
        string tenantId,
        string quizTemplateId,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.DeleteQuizTemplateAsync(userId, tenantId, quizTemplateId);

        return Results.NoContent();
    }

    private static async Task<IResult> PostQuestionSelectionRuleAsync(
        string tenantId,
        string quizTemplateId,
        CreateQuestionSelectionRuleCommand command,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(
            await service.CreateQuestionSelectionRuleAsync(
                userId,
                tenantId,
                quizTemplateId,
                command
            )
        );
    }

    private static async Task<IResult> GetQuestionSelectionRuleByIdAsync(
        string tenantId,
        string ruleId,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(
            await service.GetQuestionSelectionRuleByIdAsync(userId, tenantId, ruleId)
        );
    }

    private static async Task<IResult> GetAllQuestionSelectionRuleAsync(
        string tenantId,
        string quizTemplateId,
        [AsParameters] GetAllQuestionSelectionRuleQuery query,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(
            await service.GetAllQuestionSelectionRulesAsync(userId, tenantId, quizTemplateId, query)
        );
    }

    private static async Task<IResult> PutQuestionSelectionRuleAsync(
        string tenantId,
        string ruleId,
        UpdateQuestionSelectionRuleCommand command,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();
        await service.UpdateQuestionSelectionRuleAsync(userId, tenantId, ruleId, command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteQuestionSelectionRuleAsync(
        string tenantId,
        string ruleId,
        IQuizTemplateService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.DeleteQuestionSelectionRuleByIdAsync(userId, tenantId, ruleId);

        return Results.NoContent();
    }
}
