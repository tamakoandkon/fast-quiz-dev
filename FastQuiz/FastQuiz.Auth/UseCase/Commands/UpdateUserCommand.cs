namespace FastQuiz.Auth.UseCase.Commands;

public record UpdateUserCommand(string Username, string Password, string? Email);
