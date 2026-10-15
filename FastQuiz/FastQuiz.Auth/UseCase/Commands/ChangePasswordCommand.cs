namespace FastQuiz.Auth.UseCase.Commands;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword);
