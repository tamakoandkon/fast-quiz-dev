using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using FastQuiz.API.Test.Utils;
using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace FastQuiz.API.Test.Auth;

[NonParallelizable]
internal class PasswordSessionTests
{
    private sealed class JwtFactory : TestWebAppFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }));
        }
    }

    [Test]
    public async Task PasswordChange_RequiresOldPasswordAndInvalidatesAllOldTokens()
    {
        using var factory = new JwtFactory();
        using var client = factory.CreateClient();
        var name = "jwt_"+Guid.NewGuid().ToString("N")[..12];
        var registration = await client.PostAsJsonAsync("/auth/register", new UserRegisterCommand(name,"OldPass123",null));
        registration.StatusCode.ShouldBe(HttpStatusCode.OK);
        var account = (await registration.Content.ReadFromJsonAsync<AuthDto>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",account.AccessToken);
        (await client.GetAsync("/auth/profile")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.PutAsJsonAsync("/auth/me/password",new ChangePasswordCommand("wrong","NewPass123"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await client.PutAsJsonAsync("/auth/me/password",new ChangePasswordCommand("OldPass123","x"))).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await client.PutAsJsonAsync("/auth/me",new UpdateUserCommand(name,"NewPass123",null))).StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var change = await client.PutAsJsonAsync("/auth/me/password",new ChangePasswordCommand("OldPass123","NewPass123"));
        change.StatusCode.ShouldBe(HttpStatusCode.OK);
        var replacement = (await change.Content.ReadFromJsonAsync<AuthDto>())!;
        (await client.GetAsync("/auth/profile")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",replacement.AccessToken);
        (await client.GetAsync("/auth/profile")).StatusCode.ShouldBe(HttpStatusCode.OK);
        client.DefaultRequestHeaders.Authorization = null;
        (await client.PostAsJsonAsync("/auth/login",new UserLoginCommand(name,"OldPass123"))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("/auth/login",new UserLoginCommand(name,"NewPass123"))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.PutAsJsonAsync("/auth/me/password",new ChangePasswordCommand("NewPass123","NextPass123"))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(replacement.AccessToken);
        jwt.Claims.Single(x=>x.Type==CredentialStamp.ClaimType).Value.ShouldNotContain("$2");
    }

    [Test]
    public void CredentialStamp_IsKeyedAndRejectsMissingMalformedOrStaleVersions()
    {
        var stamp=CredentialStamp.Create("synthetic-password-hash","synthetic-signing-key");
        CredentialStamp.Matches(stamp,"synthetic-password-hash","synthetic-signing-key").ShouldBeTrue();
        CredentialStamp.Matches(stamp,"changed-hash","synthetic-signing-key").ShouldBeFalse();
        CredentialStamp.Matches(stamp,"synthetic-password-hash","different-key").ShouldBeFalse();
        CredentialStamp.Matches(null,"synthetic-password-hash","synthetic-signing-key").ShouldBeFalse();
        CredentialStamp.Matches("invalid-base64!","synthetic-password-hash","synthetic-signing-key").ShouldBeFalse();
    }
}
