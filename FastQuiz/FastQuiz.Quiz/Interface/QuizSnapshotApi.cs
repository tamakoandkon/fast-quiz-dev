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

public static class QuizSnapshotApi
{
    public static void MapQuizSnapshotEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenant/{tenantId}/quiz-template")
            .RequireAuthorization()
            .WithTags("QuizSnapshot")
            .WithStandardErrorMetadata();

        #region QuizSnapshot
        group
            .MapPost("{quizTemplateId}/quiz-snapshot", PostQuizSnapshotAsync)
            .Produces(StatusCodes.Status204NoContent);

        group
            .MapGet("{quizTemplateId}/quiz-snapshot", GetAllQuizSnapshotAsync)
            .Produces<List<QuizSnapshotDto>>(StatusCodes.Status200OK);

        group
            .MapGet("quiz-snapshot/{quizSnapshotId}", GetQuizSnapshotByIdAsync)
            .Produces<QuizSnapshotDto>(StatusCodes.Status200OK);

        group
            .MapDelete("quiz-snapshot/{quizSnapshotId}", DeleteQuizSnapshotByIdAsync)
            .Produces(StatusCodes.Status204NoContent);
        #endregion

        #region QuestionSnapshot

        group
            .MapGet(
                "quiz-snapshot/{quizSnapshotId}/question-snapshot",
                GetAllQuestionSnapshotByQuizSnapshotIdAsync
            )
            .Produces<List<QuestionSnapshotDto>>(StatusCodes.Status200OK);

        #endregion
    }

    private static async Task<IResult> PostQuizSnapshotAsync(
        string tenantId,
        string quizTemplateId,
        CreateQuizSnapshotCommand command,
        IClaimsManager claimsManager,
        IQuizSnapshotService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service
            .CreateQuizSnapshotAsync(userId, tenantId, quizTemplateId, command)
            .ConfigureAwait(false);

        return Results.NoContent();
    }

    private static async Task<IResult> GetAllQuizSnapshotAsync(
        string tenantId,
        string quizTemplateId,
        [AsParameters] GetAllQuizSnapshotQuery query,
        IClaimsManager claimsManager,
        IQuizSnapshotService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        var result = await service.GetAllQuizSnapshotAsync(userId, tenantId, quizTemplateId, query);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetQuizSnapshotByIdAsync(
        string tenantId,
        string quizSnapshotId,
        IClaimsManager claimsManager,
        IQuizSnapshotService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        var result = await service.GetQuizSnapshotByIdAsync(userId, tenantId, quizSnapshotId);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteQuizSnapshotByIdAsync(
        string tenantId,
        string quizSnapshotId,
        IClaimsManager claimsManager,
        IQuizSnapshotService service
    )
    {
        var userId = claimsManager.UserId;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        await service.DeleteQuizSnapshotByIdAsync(userId, tenantId, quizSnapshotId);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllQuestionSnapshotByQuizSnapshotIdAsync(
        string tenantId,
        string quizSnapshotId,
        [AsParameters] GetAllQuestionSnapshotQuery query,
        IClaimsManager claimsManager,
        IQuizSnapshotService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        var result = await service.GetAllQuestionSnapshotAsync(
            userId,
            tenantId,
            quizSnapshotId,
            query
        );
        return Results.Ok(result);
    }
}
