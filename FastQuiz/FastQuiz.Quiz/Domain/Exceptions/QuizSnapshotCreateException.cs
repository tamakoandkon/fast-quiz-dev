using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizSnapshotCreateException : BusinessException
{
    public QuizSnapshotCreateException() { }

    public QuizSnapshotCreateException(string? message)
        : base(message) { }

    public QuizSnapshotCreateException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
