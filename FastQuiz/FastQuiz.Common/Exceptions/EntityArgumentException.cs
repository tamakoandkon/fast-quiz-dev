namespace FastQuiz.Common.Exceptions;

public class EntityArgumentException : Exception
{
    public EntityArgumentException() { }

    public EntityArgumentException(string? message)
        : base(message) { }

    public EntityArgumentException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
