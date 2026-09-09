using FastQuiz.Common.Exceptions;

namespace FastQuiz.Auth.Domain.Exceptions;

public class UserAlreadyExistsException : BusinessException
{
    public UserAlreadyExistsException() { }

    public UserAlreadyExistsException(string? message)
        : base(message) { }

    public UserAlreadyExistsException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
