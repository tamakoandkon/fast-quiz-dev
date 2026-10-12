namespace FastQuiz.Common.Exceptions;

/// <summary>
/// 当操作者缺少权限，则会抛出该异常
/// </summary>
public class PermissionException : Exception
{
    public PermissionException() { }

    public PermissionException(string? message)
        : base(message) { }

    public PermissionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
