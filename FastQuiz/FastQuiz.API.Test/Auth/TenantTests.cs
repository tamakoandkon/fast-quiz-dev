using System.Net;
using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using Shouldly;

namespace FastQuiz.API.Test.Auth;

internal class TenantTests : ApiTestBase
{
    private static CreateTenantCommand NewCommand() =>
        new(
            Guid.NewGuid().ToString("N")[..12],
            Guid.NewGuid().ToString("N")[..12],
            Guid.NewGuid().ToString("N")[..12],
            Guid.NewGuid().ToString("N")[..12],
            null
        );

    [Test]
    public async Task PostCreateTenant_WhenAdminUsernameIsFree_ShouldCreateTenantAndAdmin()
    {
        // Arrange
        var client = GetAuthenticatiedClient(User.DefaultAdminId);
        var command = NewCommand();

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.TenantList(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TenantDto>();
        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);

        // 新建租户的管理员应该能用自己的账号登录
        var loginResponse = await client.PostAsJsonAsync(
            UrlPackage.Login(),
            new UserLoginCommand(command.AdminUsername, command.AdminPassword)
        );
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthDto>();
        auth.ShouldNotBeNull();
        auth.User.ShouldNotBeNull();

        // 租户管理员应该已经加入该租户，因此可以读到自己租户下的用户列表
        var adminClient = GetAuthenticatiedClient(auth.User.Id);
        var usersResponse = await adminClient.GetAsync(
            UrlPackage.TenantUser(result.Id) + "?Cursor=&Count=10&IsPrevious=false"
        );
        usersResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var users = await usersResponse.Content.ReadFromJsonAsync<List<UserDto>>();
        users.ShouldNotBeNull();
        users.ShouldContain(x => x.Id == auth.User.Id);
    }

    [Test]
    public async Task PostCreateTenant_WhenAdminUsernameAlreadyExists_ShouldReturnBadRequest()
    {
        // Arrange
        var client = GetAuthenticatiedClient(User.DefaultAdminId);
        var first = NewCommand();
        var firstResponse = await client.PostAsJsonAsync(UrlPackage.TenantList(), first);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 用同一个管理员用户名再建一个租户
        var second = NewCommand() with { AdminUsername = first.AdminUsername };

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.TenantList(), second);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostCreateTenant_WhenNotSystemAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var username = Guid.NewGuid().ToString("N")[..12];
        var authDto = await Sut.RegisterAsync(
            new(username, Guid.NewGuid().ToString("N")[..12], null)
        );
        var client = GetAuthenticatiedClient(authDto.User.Id);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.TenantList(), NewCommand());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    [TestCase("", "123456")]
    [TestCase("ab", "123456")]
    [TestCase("admin_ok", "")]
    [TestCase("admin_ok", "123")]
    public async Task PostCreateTenant_WhenAdminDataIsInvalid_ShouldReturnUnprocessableEntity(
        string adminUsername,
        string adminPassword
    )
    {
        // Arrange
        var client = GetAuthenticatiedClient(User.DefaultAdminId);
        var command = NewCommand() with
        {
            AdminUsername = adminUsername,
            AdminPassword = adminPassword,
        };

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.TenantList(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task AddUserToTenant_WhenCalledFromOutside_ShouldNotBeRouteable()
    {
        // Arrange
        var client = GetAuthenticatiedClient(User.DefaultAdminId);
        var target = await Sut.RegisterAsync(
            new(
                Guid.NewGuid().ToString("N")[..12],
                Guid.NewGuid().ToString("N")[..12],
                null
            )
        );

        // Act：外部已不能再直接把用户加进租户
        var response = await client.PostAsync(
            UrlPackage.TenantUserMap(Tenant.DefaultTenantId, target.User.Id),
            content: null
        );

        // Assert
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }
}
