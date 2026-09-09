namespace FastQuiz.Auth.UseCase.Commands;

public record UpdateRoleCommand(
    string Name,
    string Description,
    bool IsSystem,
    bool IsEnable,
    List<string> Permissions
);
