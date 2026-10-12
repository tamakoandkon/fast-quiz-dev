using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Queries;
using FastQuiz.Auth.UseCase.Services;
using FastQuiz.Common.Exceptions.Extensions;
using FastQuiz.Common.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FastQuiz.Auth.Interface;

public static class TenantApi
{
    public static void MapTenantEndPoints(this IEndpointRouteBuilder app)
    {
        #region Tenant
        var tenantGroup = app.MapGroup("/tenant")
            .RequireAuthorization()
            .WithTags("Tenant")
            .WithStandardErrorMetadata();

        tenantGroup
            .MapPost("", PostCreateTenantAsync)
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        tenantGroup
            .MapGet("", GetAllTenantAsync)
            .Produces<IReadOnlyList<TenantDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        tenantGroup
            .MapGet("me", GetAllMyTenantAsync)
            .Produces<IReadOnlyList<TenantDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        tenantGroup
            .MapGet("{tenantId}", GetTenantByIdAsync)
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tenantGroup
            .MapPut("{tenantId}", UpdateTenantAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tenantGroup
            .MapDelete("{tenantId}", DeleteTenantAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        tenantGroup
            .MapDelete("{tenantId}/user-map/{userId}", RemoveUserFromTenantAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        #endregion
    }

    #region Tenant Handlers
    private static async Task<IResult> PostCreateTenantAsync(
        CreateTenantCommand command,
        IClaimsManager claimsManager,
        ITenantService tenantService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await tenantService.CreateTenantAsync(userId, command));
    }

    private static async Task<IResult> GetAllTenantAsync(
        [AsParameters] GetAllTenantQuery query,
        IClaimsManager claimsManager,
        ITenantService tenantService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await tenantService.GetAllTenantsAsync(userId, query));
    }

    private static async Task<IResult> GetAllMyTenantAsync(
        [AsParameters] GetAllMyTenantQuery query,
        IClaimsManager claimsManager,
        ITenantService tenantService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await tenantService.GetMyTenantsAsync(userId, query));
    }

    private static async Task<IResult> GetTenantByIdAsync(
        string tenantId,
        IClaimsManager claimsManager,
        ITenantService tenantService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await tenantService.GetTenantByIdAsync(userId, tenantId));
    }

    private static async Task<IResult> UpdateTenantAsync(
        string tenantId,
        UpdateTenantCommand command,
        IClaimsManager claimsManager,
        ITenantService tenantService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await tenantService.UpdateTenantAsync(userId, tenantId, command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteTenantAsync(
        string tenantId,
        IClaimsManager claimsManager,
        ITenantService tenantService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();
        await tenantService.DeleteTenantAsync(userId, tenantId);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveUserFromTenantAsync(
        string tenantId,
        string userId,
        IClaimsManager claimsManager,
        ITenantService tenantService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();
        await tenantService.RemoveUserFromTenantAsync(myId, userId, tenantId);
        return Results.NoContent();
    }
    #endregion
}
