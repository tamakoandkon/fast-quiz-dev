using System.Net;
using System.Net.Http.Json;
using FastQuiz.API.Test.Utils;
using FastQuiz.API.Test.Utils.Helpers;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using Shouldly;

namespace FastQuiz.API.Test.Auth;

internal class AuthTests : ApiTestBase
{
    [Test]
    public async Task PostLogin_WhenPasswordIsCorrect_ShouldLoginSuccess()
    {
        // Arrange
        var client = Sut.CreateClient();
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        await Sut.RegisterAsync(new(username, password, null));
        var command = new UserLoginCommand(username, password);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.Login(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthDto>();
        result.ShouldNotBeNull();
        result.AccessToken.ShouldNotBeNull();
        result.User.ShouldNotBeNull();
        result.User.Username.ShouldBe(username);
    }

    [Test]
    public async Task PostLogin_WhenPasswordIsIncorrect_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        await Sut.RegisterAsync(new(username, password, null));
        var command = new UserLoginCommand(username, Guid.NewGuid().ToString("N")[..12]);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.Login(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PostLogin_WhenUserDoesNotExist_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var command = new UserLoginCommand(username, password);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.Login(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    [TestCase("", "")]
    [TestCase("123451234512345123451234512345123451234512345123451234512345", "12345")]
    [TestCase("12345", "123451234512345123451234512345123451234512345123451234512345")]
    public async Task PostLogin_WhenWithInvalidData_ShouldReturnUnauthorized(
        string username,
        string password
    )
    {
        // Arrange
        var client = Sut.CreateClient();
        var command = new UserLoginCommand(username, password);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.Login(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PostRegister_WhenUserDoesNotExist_ShouldRegisterSuccess()
    {
        // Arrange
        var client = Sut.CreateClient();
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];

        var command = new UserRegisterCommand(username, password, null);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.Register(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthDto>();
        result.ShouldNotBeNull();
        result.AccessToken.ShouldNotBeNull();
        result.User.ShouldNotBeNull();
        result.User.Username.ShouldBe(username);
    }

    [Test]
    public async Task PostRegister_WhenUserDoesAlreadyExist_ShouldReturnBadRequest()
    {
        // Arrange
        var client = Sut.CreateClient();
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var command = new UserRegisterCommand(username, password, null);
        await Sut.RegisterAsync(command);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.Register(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    [TestCase("", "")]
    [TestCase("123451234512345123451234512345123451234512345123451234512345", "12345")]
    [TestCase("12345", "123451234512345123451234512345123451234512345123451234512345")]
    public async Task PostRegister_WhenWithInvalidData_ShouldReturnBadRequest(
        string username,
        string password
    )
    {
        // Arrange
        var client = Sut.CreateClient();
        var command = new UserRegisterCommand(username, password, null);

        // Act
        var response = await client.PostAsJsonAsync(UrlPackage.Register(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task GetMyProfile_WhenUserDoesAlreadyEixst_ShouldReturnUserDto()
    {
        // Arrange

        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var authDto = await Sut.RegisterAsync(new(username, password, null));
        var client = Sut.CreateClient(authDto.User.Id);

        // Act
        var response = await client.GetAsync(UrlPackage.MyProfile());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(authDto.User.Id);
        result.Username.ShouldBe(username);
    }

    [Test]
    public async Task GetMyProfile_WhenUserDoesNotEixst_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = Sut.CreateClient();

        // Act
        var response = await client.GetAsync(UrlPackage.MyProfile());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetMyProfile_WhenUserHasEmail_ShouldReturnEmail()
    {
        // Arrange
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var email = $"{username}@example.com";
        var authDto = await Sut.RegisterAsync(new(username, password, email));
        var client = Sut.CreateClient(authDto.User.Id);

        // Act
        var response = await client.GetAsync(UrlPackage.MyProfile());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserDto>();
        result.ShouldNotBeNull();
        result.Email.ShouldBe(email);
        result.CreatedAt.ShouldNotBeNull();
    }

    [Test]
    public async Task PutMyAccount_WhenPasswordIsEmpty_ShouldKeepExistingPassword()
    {
        // Arrange
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var authDto = await Sut.RegisterAsync(new(username, password, null));
        var client = Sut.CreateClient(authDto.User.Id);
        var command = new UpdateUserCommand(username, "", $"{username}@example.com");

        // Act
        var response = await client.PutAsJsonAsync(UrlPackage.MyAccount(), command);

        // Assert
        // 密码留空表示不修改密码，原密码必须仍然可用
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var loginClient = Sut.CreateClient();
        var loginResponse = await loginClient.PostAsJsonAsync(
            UrlPackage.Login(),
            new UserLoginCommand(username, password)
        );
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task PutMyAccount_WhenPasswordIsProvided_ShouldRejectAndKeepPassword()
    {
        // Arrange
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var newPassword = Guid.NewGuid().ToString("N")[..12];
        var authDto = await Sut.RegisterAsync(new(username, password, null));
        var client = Sut.CreateClient(authDto.User.Id);
        var command = new UpdateUserCommand(username, newPassword, null);

        // Act
        var response = await client.PutAsJsonAsync(UrlPackage.MyAccount(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);

        var loginClient = Sut.CreateClient();
        var loginResponse = await loginClient.PostAsJsonAsync(
            UrlPackage.Login(),
            new UserLoginCommand(username, password)
        );
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    [TestCase("ab")]
    [TestCase("123451234512345123451234512345123451234512345123451234512345")]
    [TestCase("invalid-name!")]
    public async Task PutMyAccount_WhenUsernameIsInvalid_ShouldReturnUnprocessableEntity(
        string username
    )
    {
        // Arrange
        var originUsername = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var authDto = await Sut.RegisterAsync(new(originUsername, password, null));
        var client = Sut.CreateClient(authDto.User.Id);
        var command = new UpdateUserCommand(username, "", null);

        // Act
        var response = await client.PutAsJsonAsync(UrlPackage.MyAccount(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task PutMyAccount_WhenEmailIsInvalid_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        var username = Guid.NewGuid().ToString("N")[..12];
        var password = Guid.NewGuid().ToString("N")[..12];
        var authDto = await Sut.RegisterAsync(new(username, password, null));
        var client = Sut.CreateClient(authDto.User.Id);
        var command = new UpdateUserCommand(username, "", "not-an-email");

        // Act
        var response = await client.PutAsJsonAsync(UrlPackage.MyAccount(), command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }
}
