using FastQuiz.Common.Exceptions;

namespace FastQuiz.Auth.Domain.Exceptions;

public class CreateUserException : BusinessException
{
    public CreateUserException() { }

    public CreateUserException(string? message)
        : base(message) { }

    public CreateUserException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
