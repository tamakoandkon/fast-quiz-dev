using FastQuiz.Auth.UseCase.Dtos;

namespace FastQuiz.Auth.UseCase.Services;

public class AuthResult
{
    public bool IsAuthentication { get; set; }
    public AuthDto? Dto { get; set; }
}
