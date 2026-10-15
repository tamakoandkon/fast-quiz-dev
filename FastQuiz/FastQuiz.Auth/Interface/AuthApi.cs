using System.Security.Claims;
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

public static class AuthApi
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth").WithStandardErrorMetadata();

        group.MapPost("/register", RegisterUserAsync).Produces<AuthDto>(StatusCodes.Status200OK);

        group
            .MapPost("/login", LoginAsync)
            .Produces<AuthDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group
            .MapGet("/profile", GetProfile)
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/me", PutMeAsync).RequireAuthorization().Produces(StatusCodes.Status204NoContent);
        group.MapPut("/me/password", ChangePasswordAsync).RequireAuthorization().Produces<AuthDto>();

        var userGroup = app.MapGroup("/tenant")
            .RequireAuthorization()
            .WithTags("User")
            .WithStandardErrorMetadata();

        userGroup
            .MapPost("{tenantId}/user", PostCreateUserAsync)
            .Produces<UserDto>(StatusCodes.Status200OK);

        userGroup
            .MapGet("{tenantId}/user/{userId}", GetUserByIdAsync)
            .Produces<UserDto>(StatusCodes.Status200OK);

        userGroup
            .MapGet("{tenantId}/user", GetUserByTenantId)
            .Produces<List<UserDto>>(StatusCodes.Status200OK);

        userGroup
            .MapGet("organization/{orgId}/user", GetUserByOrganizationId)
            .Produces<List<UserDto>>(StatusCodes.Status200OK);

        userGroup
            .MapGet("organization/{orgId}/user/not-in", GetNotInItUserByOrganizationIdAsync)
            .Produces<List<UserDto>>(StatusCodes.Status200OK);

        userGroup
            .MapPut("{tenantId}/user/{userId}", PutUserByIdAsync)
            .Produces<UserDto>(StatusCodes.Status200OK);

        userGroup
            .MapDelete("{tenantId}/user/{userId}", DeleteUserByIdAsync)
            .Produces<UserDto>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RegisterUserAsync(
        UserRegisterCommand command,
        IUserApplicationService service
    )
    {
        return Results.Ok(await service.RegisterAsync(command));
    }

    private static async Task<IResult> LoginAsync(
        UserLoginCommand command,
        IUserApplicationService service
    )
    {
        var authResult = await service.LoginAsync(command);

        if (!authResult.IsAuthentication || authResult.Dto == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(authResult.Dto);
    }

    private static async Task<IResult> GetProfile(
        ClaimsPrincipal userPrincipal,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetProfileAsync(userId));
    }

    private static async Task<IResult> GetUserByTenantId(
        string tenantId,
        [AsParameters] GetUsersByTenantIdQuery query,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetUsersByTenantIdAsync(userId, tenantId, query));
    }

    private static async Task<IResult> GetUserByOrganizationId(
        string orgId,
        [AsParameters] GetUsersByOrganizationIdQuery query,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetUsersByOrganizationIdAsync(userId, orgId, query));
    }

    private static async Task<IResult> GetNotInItUserByOrganizationIdAsync(
        string orgId,
        [AsParameters] GetUsersByOrganizationIdQuery query,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetNotInItUsersByOrganizationIdAsync(userId, orgId, query));
    }

    private static async Task<IResult> PostCreateUserAsync(
        string tenantId,
        UserRegisterCommand command,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var userId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.CreateUserAsync(userId, tenantId, command));
    }

    private static async Task<IResult> GetUserByIdAsync(
        string tenantId,
        string userId,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();

        return Results.Ok(await service.GetUserByIdAsync(myId, tenantId, userId));
    }

    private static async Task<IResult> PutUserByIdAsync(
        string tenantId,
        string userId,
        UpdateUserCommand command,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();
        await service.UpdateUserByIdAsync(myId, tenantId, userId, command);
        return Results.NoContent();
    }

    private static async Task<IResult> PutMeAsync(
        UpdateUserCommand command,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();
        await service.UpdateUserByIdAsync(myId, command);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordCommand command, IClaimsManager claimsManager, IUserApplicationService service
    ) => Results.Ok(await service.ChangePasswordAsync(claimsManager.EnsureAndGetUserId(), command));

    private static async Task<IResult> DeleteUserByIdAsync(
        string tenantId,
        string userId,
        IClaimsManager claimsManager,
        IUserApplicationService service
    )
    {
        var myId = claimsManager.EnsureAndGetUserId();
        await service.DeleteUserByIdAsync(myId, tenantId, userId);
        return Results.NoContent();
    }
}
