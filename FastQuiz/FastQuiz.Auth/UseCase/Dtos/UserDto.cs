namespace FastQuiz.Auth.UseCase.Dtos;

public record UserDto(string Id, string Username, string? Email = null, DateTimeOffset? CreatedAt = null);
