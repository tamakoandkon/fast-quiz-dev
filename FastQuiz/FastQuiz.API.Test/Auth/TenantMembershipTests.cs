using System.Net;
using System.Net.Http.Json;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace FastQuiz.API.Test.Auth;

internal partial class TenantTests
{
    [Test]
    public async Task ExistingAccountCanJoinAnotherTenantWithoutChangingCredentialsOrGrantingRoles()
    {
        var command = NewCommand();
        var admin = GetAuthenticatiedClient(User.DefaultAdminId);
        var tenantResponse = await admin.PostAsJsonAsync("/tenant", command);
        var tenant = (await tenantResponse.Content.ReadFromJsonAsync<TenantDto>())!;
        var username = Guid.NewGuid().ToString("N")[..16];
        var user = await Sut.RegisterAsync(new(username, "member-password", null));
        await using var scope = Sut.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        await repository.AddUserToTenantAsync(Tenant.DefaultTenantId, user.User.Id);
        var url = $"/tenant/{tenant.Id}/members";

        var responses = await Task.WhenAll(
            admin.PostAsJsonAsync(url, new AddTenantMemberCommand(username)),
            admin.PostAsJsonAsync(url, new AddTenantMemberCommand(username)));
        foreach (var response in responses) response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await repository.GetUserIdsByTenantIdAsync(tenant.Id)).Count(x => x == user.User.Id).ShouldBe(1);
        (await repository.GetUserInTenantAsync(Tenant.DefaultTenantId, user.User.Id)).ShouldBeTrue();
        (await repository.GetUserInTenantAsync(tenant.Id, user.User.Id)).ShouldBeTrue();
        (await scope.ServiceProvider.GetRequiredService<IPermissionService>()
            .HasPermissionAsync(tenant.Id, user.User.Id, Permissions.User)).ShouldBeFalse();
        var login = await admin.PostAsJsonAsync("/auth/login", new UserLoginCommand(username, "member-password"));
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        var member = GetAuthenticatiedClient(user.User.Id);
        var myTenants = await member.GetFromJsonAsync<List<TenantDto>>("/tenant/me?Cursor=&Count=100&IsPrevious=false");
        myTenants!.ShouldContain(x => x.Id == tenant.Id);
    }

    [Test]
    public async Task TenantAdministratorCannotLookUpOrAddGlobalAccounts()
    {
        var command = NewCommand();
        var response = await GetAuthenticatiedClient(User.DefaultAdminId).PostAsJsonAsync("/tenant", command);
        var tenant = (await response.Content.ReadFromJsonAsync<TenantDto>())!;
        var login = await GetClient().PostAsJsonAsync("/auth/login",
            new UserLoginCommand(command.AdminUsername, command.AdminPassword));
        var auth = (await login.Content.ReadFromJsonAsync<AuthDto>())!;
        var client = GetAuthenticatiedClient(auth.User.Id);
        foreach (var username in new[] { "defaultadmin", "unknown-account" })
            (await client.PostAsJsonAsync($"/tenant/{tenant.Id}/members", new AddTenantMemberCommand(username)))
                .StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [TestCase("", HttpStatusCode.BadRequest)]
    [TestCase("unknown_account", HttpStatusCode.NotFound)]
    public async Task InvalidOrUnknownExistingUsernameIsRejected(string username, HttpStatusCode expected)
    {
        var response = await GetAuthenticatiedClient(User.DefaultAdminId).PostAsJsonAsync(
            $"/tenant/{Tenant.DefaultTenantId}/members", new AddTenantMemberCommand(username));
        response.StatusCode.ShouldBe(expected);
    }

    [Test]
    public async Task MembershipEndpointRejectsUnknownTenantAndUnauthenticatedRequests()
    {
        (await GetAuthenticatiedClient(User.DefaultAdminId).PostAsJsonAsync(
            "/tenant/missing/members", new AddTenantMemberCommand("defaultadmin")))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await GetClient().PostAsJsonAsync(
            $"/tenant/{Tenant.DefaultTenantId}/members", new AddTenantMemberCommand("defaultadmin")))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
