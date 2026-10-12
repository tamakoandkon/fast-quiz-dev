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

public static class RoleApi
{
    public static void MapRoleEndPoints(this IEndpointRouteBuilder app)
    {
        #region Role
        var roleGroup = app.MapGroup("/tenant")
            .RequireAuthorization()
            .WithTags("Role")
            .WithStandardErrorMetadata();

        roleGroup
            .MapPost("{tenantId}/role", PostCreateRoleAsync)
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        roleGroup
            .MapGet("role/{roleId}", GetRoleByIdAsync)
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        roleGroup
            .MapGet("{tenantId}/role", GetRoleByTenantIdAsync)
            .Produces<IReadOnlyList<RoleDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        roleGroup
            .MapPut("role/{roleId}", UpdateRoleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        roleGroup
            .MapDelete("role/{roleId}", DeleteRoleAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        roleGroup
            .MapPost("role/{roleId}/permission/{permission}", AddPermissionToRoleAsync)
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        roleGroup
            .MapDelete("role/{roleId}/permission/{permission}", RemovePermissionFromRole)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        roleGroup
            .MapPost("role/{roleId}/user/{userId}", AddRoleToUserAsync)
            .Produces<RoleDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        roleGroup
            .MapGet("{tenantId}/role/user/{userId}", GetAllUserRolesAsync)
            .Produces<List<RoleDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        roleGroup
            .MapDelete("role/{roleId}/user/{userId}", RemoveRoleFromUserAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        roleGroup
            .MapGet("role/permission", GetAllPermissionsAsync)
            .Produces<List<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        roleGroup
            .MapGet("{tenantId}/role/permission/me", GetMyPermissionsAsync)
            .Produces<List<string>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        #endregion
    }

    #region Role Handlers
    private static async Task<IResult> PostCreateRoleAsync(
        string tenantId,
        CreateRoleCommand command,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await roleService.CreateAsync(userId, tenantId, command));
    }

    private static async Task<IResult> GetRoleByIdAsync(
        string roleId,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await roleService.GetRoleByIdAsync(userId, roleId));
    }

    private static async Task<IResult> GetRoleByTenantIdAsync(
        string tenantId,
        [AsParameters] GetAllRoleQuery query,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await roleService.GetAllRolesAsync(userId, tenantId, query));
    }

    private static async Task<IResult> UpdateRoleAsync(
        string roleId,
        UpdateRoleCommand command,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();
        await roleService.UpdateAsync(userId, roleId, command);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteRoleAsync(
        string roleId,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();
        await roleService.DeleteRoleByIdAsync(userId, roleId);
        return Results.NoContent();
    }

    private static async Task<IResult> AddPermissionToRoleAsync(
        string permission,
        string roleId,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await roleService.AddPermissionToRoleAsync(userId, roleId, permission);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllUserRolesAsync(
        string tenantId,
        string userId,
        [AsParameters] GetAllUserRolesQuery query,
        IClaimsManager claimsManager,
        IPermissionService permissionService,
        IRoleService roleService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();

        var result = await roleService.GetAllUserRolesAsync(myId, tenantId, userId, query);

        return Results.Ok(result);
    }

    private static async Task<IResult> RemovePermissionFromRole(
        string permission,
        string roleId,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        await roleService.RemovePermissionFromRoleAsync(userId, roleId, permission);
        return Results.NoContent();
    }

    private static async Task<IResult> AddRoleToUserAsync(
        string userId,
        string roleId,
        IClaimsManager claimsManager,
        IPermissionService permissionService,
        IRoleService roleService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();

        await roleService.AddRoleToUserAsync(myId, userId, roleId);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveRoleFromUserAsync(
        string userId,
        string roleId,
        IClaimsManager claimsManager,
        IPermissionService permissionService,
        IRoleService roleService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();

        await roleService.RemoveRoleFromUserAsync(myId, userId, roleId);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAllPermissionsAsync(
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();
        return Results.Ok(await roleService.GetAllPermissionsAsync(myId));
    }

    private static async Task<IResult> GetMyPermissionsAsync(
        string tenantId,
        IClaimsManager claimsManager,
        IRoleService roleService
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();
        return Results.Ok(await roleService.GetPermissionsAsync(tenantId, myId));
    }

    #endregion
}
