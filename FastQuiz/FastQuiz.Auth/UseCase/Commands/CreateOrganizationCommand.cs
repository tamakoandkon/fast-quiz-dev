namespace FastQuiz.Auth.UseCase.Commands;

public record CreateOrganizationCommand(string Name, string? ParentId);
