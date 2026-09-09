using FastQuiz.Common.Exceptions;

namespace FastQuiz.Auth.Domain.Exceptions;

public class RoleNotFoundExcpetion : ResourceNotFoundException
{
    public RoleNotFoundExcpetion() { }

    public RoleNotFoundExcpetion(string? message)
        : base(message) { }

    public RoleNotFoundExcpetion(string? message, Exception? innerException)
        : base(message, innerException) { }
}
