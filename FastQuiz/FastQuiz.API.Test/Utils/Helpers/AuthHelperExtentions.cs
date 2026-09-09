using FastQuiz.Auth.UseCase.Commands;
using FastQuiz.Auth.UseCase.Dtos;
using FastQuiz.Auth.UseCase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FastQuiz.API.Test.Utils.Helpers;

internal static class AuthHelperExtentions
{
    public static async Task<AuthDto> RegisterAsync(
        this TestWebAppFactory sut,
        UserRegisterCommand command
    )
    {
        await using var scope = sut.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IUserApplicationService>();
        return await service.RegisterAsync(command);
    }
}
