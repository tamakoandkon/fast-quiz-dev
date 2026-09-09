using FastQuiz.Common.Exceptions.Extensions;
using FastQuiz.Common.Utils;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;
using FastQuiz.Quiz.UseCase.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FastQuiz.Quiz.Interface;

public static class QuizAttemptApi
{
    public static void MapQuizAttemptApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenant/{tenantId}/quiz-template")
            .RequireAuthorization()
            .WithTags("QuizAttempt")
            .WithStandardErrorMetadata();

        #region QuizAttempt
        group
            .MapPost("{quizTemplateId}/quiz-attempt", PostQuizAttemptAsync)
            .Produces<QuizAttempDto>(StatusCodes.Status200OK);

        group
            .MapPost("{quizTemplateId}/quiz-attempt/batch", PostBatchQuizAttemptAsync)
            .Produces(StatusCodes.Status204NoContent);

        group
            .MapGet("{quizTemplateId}/quiz-attempt", GetAllQuizAttemptAsync)
            .Produces<List<QuizAttempDto>>(StatusCodes.Status200OK);

        group
            .MapGet("quiz-attempt/me", GetMyQuizAttemptsAsync)
            .Produces<List<QuizAttempDto>>(StatusCodes.Status200OK);

        group
            .MapGet("quiz-attempt/{quizAttemptId}", GetQuizAttemptByIdAsync)
            .Produces<QuizAttempDto>(StatusCodes.Status200OK);

        //group
        //    .MapPost("quiz-attempt/{quizAttemptId}", PostAssignQuizAttemptAsync)
        //    .Produces(StatusCodes.Status204NoContent);

        group
            .MapPut("quiz-attempt/{quizAttemptId}", PutQuizAttemptStatusAsync)
            .Produces(StatusCodes.Status204NoContent);

        #endregion

        #region QuestionRecord
        group
            .MapGet(
                "quiz-attempt/{quizAttemptId}/question",
                GetQuestionWithOutAnswerByAttemptIdAsync
            )
            .Produces<List<QuestionRecordWithoutAnswerDto>>(StatusCodes.Status200OK);

        group
            .MapGet(
                "quiz-attempt/{quizAttemptId}/question/with-answer",
                GetQuestionWithAnswerByAttemptIdAsync
            )
            .Produces<List<QuestionRecordWithAnswerDto>>(StatusCodes.Status200OK);

        group
            .MapPut("quiz-attempt/question/{questionId}", PutAnswerQuestionAsync)
            .Produces(StatusCodes.Status204NoContent);

        group
            .MapPut(
                "quiz-attempt/question/{questionId}/status/{isCorrect}",
                PutReviewQuestionRecordAsync
            )
            .Produces(StatusCodes.Status204NoContent);
        #endregion
    }

    #region QuizAttempt

    public static async Task<IResult> PostQuizAttemptAsync(
        string tenantId,
        string quizTemplateId,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.CreateQuizAttemptAsync(userId, tenantId, quizTemplateId));
    }

    public static async Task<IResult> PostBatchQuizAttemptAsync(
        string tenantId,
        string quizTemplateId,
        List<CreateQuizAttemptCommand> commands,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.CreateQuizAttemptAsync(userId, tenantId, quizTemplateId, commands);

        return Results.NoContent();
    }

    public static async Task<IResult> GetAllQuizAttemptAsync(
        string tenantId,
        string quizTemplateId,
        [AsParameters] GetAllQuizAttemptQuery query,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.UserId;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var result = await service.GetAllQuizAttemptAsync(userId, tenantId, quizTemplateId, query);

        return Results.Ok(result);
    }

    public static async Task<IResult> GetMyQuizAttemptsAsync(
        string tenantId,
        [AsParameters] GetMyQuizAttemptQuery query,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        var result = await service.GetMyQuizAttemptAsync(userId, tenantId, query);

        return Results.Ok(result);
    }

    public static async Task<IResult> GetQuizAttemptByIdAsync(
        string tenantId,
        string quizAttemptId,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.UserId;
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        var result = await service.GetQuizAttemptByIdAsync(userId, tenantId, quizAttemptId);

        return Results.Ok(result);
    }

    public static async Task<IResult> PutQuizAttemptStatusAsync(
        string tenantId,
        string quizAttemptId,
        QuizAttemptStatus status,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.UpdateQuizAttemptStatusAsync(userId, tenantId, quizAttemptId, status);
        return Results.NoContent();
    }

    #endregion

    #region QuestionRecord

    public static async Task<IResult> GetQuestionWithOutAnswerByAttemptIdAsync(
        string tenantId,
        string quizAttemptId,
        [AsParameters] GetAllQuestionRecordDtoQuery query,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();
        var result = await service.GetAllQuestionWithoutAnswerDtoAsync(
            userId,
            tenantId,
            quizAttemptId,
            query
        );

        return Results.Ok(result);
    }

    public static async Task<IResult> GetQuestionWithAnswerByAttemptIdAsync(
        string tenantId,
        string quizAttemptId,
        [AsParameters] GetAllQuestionRecordDtoQuery query,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();
        var result = await service.GetAllQuestionWithAnswerDtoAsync(
            userId,
            tenantId,
            quizAttemptId,
            query
        );

        return Results.Ok(result);
    }

    public static async Task<IResult> PutAnswerQuestionAsync(
        string tenantId,
        string questionId,
        List<SetAnswerRecordCommand> commands,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.AnswerQuestionAsync(userId, tenantId, questionId, commands);

        return Results.NoContent();
    }

    public static async Task<IResult> PutReviewQuestionRecordAsync(
        string tenantId,
        string questionId,
        bool isCorrect,
        IQuizAtteptService service,
        IClaimsManager claimsManager
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await service.ManualReviewQuestionRecordAsync(tenantId, userId, questionId, isCorrect);

        return Results.NoContent();
    }

    #endregion
}
