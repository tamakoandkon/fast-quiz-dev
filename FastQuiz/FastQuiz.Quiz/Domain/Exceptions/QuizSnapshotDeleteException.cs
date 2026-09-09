using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizSnapshotDeleteException : BusinessException
{
    public QuizSnapshotDeleteException() { }

    public QuizSnapshotDeleteException(string? message)
        : base(message) { }

    public QuizSnapshotDeleteException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
