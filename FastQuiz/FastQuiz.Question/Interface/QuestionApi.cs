using FastQuiz.Common.Exceptions.Extensions;
using FastQuiz.Common.Utils;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Question.Infrastructure.Repositories.Sqlite;
using FastQuiz.Question.UseCase.Command;
using FastQuiz.Question.UseCase.Dto;
using FastQuiz.Question.UseCase.Query;
using FastQuiz.Question.UseCase.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FastQuiz.Question.Interface;

public static class QuestionApi
{
    public static void MapQuestionGroupApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenant/{tenantId}/question-group")
            .RequireAuthorization()
            .WithTags("QuestionGroup")
            .WithStandardErrorMetadata();

        #region Question Group
        group
            .MapPost("", PostCreateQuestionGroupAsync)
            .Produces<QuestionGroupDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost("batch-delete", DeleteManyQuestionGroupsAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapGet("{groupId}", GetQuestionGroupByIdAsync)
            .Produces<QuestionGroupDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapGet("{groupId}/count", GetQuestionCountByIdAsync)
            .Produces<int>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapGet("", GetAllQuestionGroupAsync)
            .Produces<IReadOnlyList<QuestionGroupDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group
            .MapPut("{groupId}", UpdateQuestionGroupAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapDelete("{groupId}", DeleteQuestionGroupAysnc)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        #endregion
        #region Question
        // Questions
        group
            .MapPost("{groupId}/question", PostCreateQuestionAsync)
            .Produces<QuestionDto>(StatusCodes.Status200OK);

        group
            .MapGet("question/{questionId}", GetQuestionAsync)
            .Produces<QuestionDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapGet("{questionGroupId}/question", GetAllQuestionAsync)
            .Produces<List<QuestionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group
            .MapPut("question/{questionId}", PutQuestionAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapDelete("question/{questionId}", DeleteQuestionAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        #endregion
    }

    #region QuestionGroup Handler
    private static async Task<IResult> PostCreateQuestionGroupAsync(
        string tenantId,
        CreateQuestionGroupCommand command,
        IQuestionGroupService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.CreateQuestionGroupAsync(userId, tenantId, command));
    }

    private static async Task<IResult> GetQuestionGroupByIdAsync(
        string tenantId,
        string groupId,
        IQuestionGroupService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetQuestionGroupByIdAsync(userId, tenantId, groupId));
    }

    private static async Task<IResult> GetQuestionCountByIdAsync(
        string tenantId,
        string groupId,
        QuestionType type,
        int level,
        IQuestionGroupService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(
            await service.GetQuestionCountByIdAsync(userId, tenantId, groupId, type, level)
        );
    }

    private static async Task<IResult> GetAllQuestionGroupAsync(
        string tenantId,
        [AsParameters] GetAllQuestionGroupQuery query,
        SqliteQuestionDbContext db,
        IQuestionGroupService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetAllQuestionGroupAsync(userId, tenantId, query));
    }

    private static async Task<IResult> UpdateQuestionGroupAsync(
        string tenantId,
        string groupId,
        UpdateQuestionGroupCommand command,
        IQuestionGroupService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.UpdateQuestionGroupAsync(userId, tenantId, groupId, command);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteQuestionGroupAysnc(
        string tenantId,
        string groupId,
        IQuestionGroupService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.DeleteQuestionGroupByIdAsync(userId, tenantId, groupId);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteManyQuestionGroupsAsync(
        string tenantId,
        DeleteQuestionGroupCommand command,
        IQuestionGroupService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.UserId;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        await service.DeleteQuestionGroupByIdAsync(userId, tenantId, command.GroupIds);
        return Results.NoContent();
    }
    #endregion

    #region Question Handler
    private static async Task<IResult> PostCreateQuestionAsync(
        string tenantId,
        string groupId,
        CreateQuestionCommand command,
        IQuestionService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.CreateQuestionAsync(userId, tenantId, groupId, command));
    }

    private static async Task<IResult> GetQuestionAsync(
        string tenantId,
        string questionId,
        IQuestionService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetQuestionByIdAsync(userId, tenantId, questionId));
    }

    private static async Task<IResult> GetAllQuestionAsync(
        string tenantId,
        string questionGroupId,
        [AsParameters] GetAllQuestionQuery query,
        IQuestionService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(
            await service.GetAllQuestionAsync(userId, tenantId, questionGroupId, query)
        );
    }

    private static async Task<IResult> PutQuestionAsync(
        string tenantId,
        string questionId,
        UpdateQuestionCommand command,
        IQuestionService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.UpdateQuestionAsync(userId, tenantId, questionId, command);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteQuestionAsync(
        string tenantId,
        string questionId,
        SqliteQuestionDbContext db,
        IQuestionService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.DeleteByIdAsync(userId, tenantId, questionId);

        return Results.NoContent();
    }

    #endregion
}
