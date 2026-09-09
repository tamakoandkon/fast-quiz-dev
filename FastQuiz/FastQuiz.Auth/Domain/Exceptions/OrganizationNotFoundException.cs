using FastQuiz.Common.Exceptions;

namespace FastQuiz.Auth.Domain.Exceptions;

public class OrganizationNotFoundException : ResourceNotFoundException
{
    public OrganizationNotFoundException() { }

    public OrganizationNotFoundException(string? message)
        : base(message) { }

    public OrganizationNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
