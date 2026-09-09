namespace FastQuiz.Auth.UseCase.Commands;

public record UserRegisterCommand(string Username, string Password, string? Email);
