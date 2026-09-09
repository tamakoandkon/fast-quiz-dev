using FastQuiz.Common.Exceptions;

namespace FastQuiz.Auth.Domain.Exceptions;

public class TenantUserMapExistsException : BusinessException
{
    public TenantUserMapExistsException() { }

    public TenantUserMapExistsException(string? message)
        : base(message) { }

    public TenantUserMapExistsException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
