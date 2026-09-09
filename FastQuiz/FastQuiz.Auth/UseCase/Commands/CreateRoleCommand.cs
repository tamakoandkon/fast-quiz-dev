namespace FastQuiz.Auth.UseCase.Commands;

public record CreateRoleCommand(
    string Name,
    string Description,
    bool IsSystem,
    bool IsEnable,
    List<string> Permissions
);
