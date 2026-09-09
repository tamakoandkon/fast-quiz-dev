using FastQuiz.Common.Exceptions;

namespace FastQuiz.Auth.Domain.Exceptions;

public class UserNotFoundException : ResourceNotFoundException
{
    public UserNotFoundException() { }

    public UserNotFoundException(string? message)
        : base(message) { }

    public UserNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
