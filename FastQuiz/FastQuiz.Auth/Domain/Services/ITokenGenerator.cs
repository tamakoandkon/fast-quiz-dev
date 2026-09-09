using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.Auth.Domain.Services;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}
