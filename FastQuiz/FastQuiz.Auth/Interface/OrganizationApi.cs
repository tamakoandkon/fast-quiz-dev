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

public static class OrganizationApi
{
    public static void MapOrganizationApi(this IEndpointRouteBuilder app)
    {
        #region Organization
        var orgGroup = app.MapGroup("/tenant")
            .RequireAuthorization()
            .WithTags("Organization")
            .WithStandardErrorMetadata();

        orgGroup
            .MapPost("{tenantId}/organization", PostCreateOrganizationAsync)
            .Produces<OrganizationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        orgGroup
            .MapGet("{tenantId}/organization", GetAllOrganizationAsync)
            .Produces<IReadOnlyList<OrganizationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        orgGroup
            .MapGet("{tenantId}/organization/{parentId}", GetAllOrganizationByParentIdAsync)
            .Produces<IReadOnlyList<OrganizationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        orgGroup
            .MapGet("organization/{orgId}", GetOrganizationByIdAsync)
            .Produces<OrganizationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        orgGroup
            .MapPut("organization/{orgId}", UpdateOrganizationAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        orgGroup
            .MapDelete("organization/{orgId}", DeleteOrganizationAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        orgGroup
            .MapPost("organization/{orgId}/user/{userId}", AddUserToOrganizationAsync)
            .Produces<OrganizationDto>(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        orgGroup
            .MapDelete("organization{orgId}/user/{userId}", RemoveUserFromOrganizationAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
        #endregion
    }

    #region Organization Handlers
    private static async Task<IResult> PostCreateOrganizationAsync(
        string tenantId,
        CreateOrganizationCommand command,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await organizationService.CreateAsync(userId, tenantId, command));
    }

    private static async Task<IResult> GetAllOrganizationAsync(
        string tenantId,
        [AsParameters] GetAllOrganizationQuery query,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await organizationService.GetAllAsync(userId, tenantId, query));
    }

    private static async Task<IResult> GetAllOrganizationByParentIdAsync(
        string tenantId,
        string parentId,
        [AsParameters] GetAllOrganizationQuery query,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(
            await organizationService.GetAllByParentIdAsync(userId, tenantId, parentId, query)
        );
    }

    private static async Task<IResult> GetOrganizationByIdAsync(
        string orgId,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await organizationService.GetByIdAsync(userId, orgId));
    }

    private static async Task<IResult> UpdateOrganizationAsync(
        string orgId,
        UpdateOrganizationCommand command,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await organizationService.UpdateAsync(userId, orgId, command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteOrganizationAsync(
        string orgId,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await organizationService.DeleteByIdAsync(userId, orgId);
        return Results.NoContent();
    }

    private static async Task<IResult> AddUserToOrganizationAsync(
        string orgId,
        string userId,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();

        await organizationService.AddUserToOrganzitionAsync(myId, userId, orgId);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveUserFromOrganizationAsync(
        string orgId,
        string userId,
        IClaimsManager claimsManager,
        IOrganizationService organizationService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();

        await organizationService.RemoveUserFromOrganizationAsync(myId, userId, orgId);
        return Results.NoContent();
    }
    #endregion
}
