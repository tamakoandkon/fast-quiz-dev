using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizAttemptCreateException : BusinessException
{
    public QuizAttemptCreateException() { }

    public QuizAttemptCreateException(string? message)
        : base(message) { }

    public QuizAttemptCreateException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
