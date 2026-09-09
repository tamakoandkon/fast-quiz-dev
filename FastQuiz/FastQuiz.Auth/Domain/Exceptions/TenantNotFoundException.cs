using FastQuiz.Common.Exceptions;

namespace FastQuiz.Auth.Domain.Exceptions;

public class TenantNotFoundException : ResourceNotFoundException
{
    public TenantNotFoundException() { }

    public TenantNotFoundException(string? message)
        : base(message) { }

    public TenantNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
