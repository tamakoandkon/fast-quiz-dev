namespace FastQuiz.Auth.UseCase.Dtos;

public record AuthDto(string AccessToken, long ExpiresIn, UserDto User);
