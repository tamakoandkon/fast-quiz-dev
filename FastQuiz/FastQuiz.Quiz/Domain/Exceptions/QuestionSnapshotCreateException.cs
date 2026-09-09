using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuestionSnapshotCreateException : BusinessException
{
    public QuestionSnapshotCreateException() { }

    public QuestionSnapshotCreateException(string? message)
        : base(message) { }

    public QuestionSnapshotCreateException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
