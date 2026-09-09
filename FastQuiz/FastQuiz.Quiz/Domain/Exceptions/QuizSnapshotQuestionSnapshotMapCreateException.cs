using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizSnapshotQuestionSnapshotMapCreateException : BusinessException
{
    public QuizSnapshotQuestionSnapshotMapCreateException() { }

    public QuizSnapshotQuestionSnapshotMapCreateException(string? message)
        : base(message) { }

    public QuizSnapshotQuestionSnapshotMapCreateException(
        string? message,
        Exception? innerException
    )
        : base(message, innerException) { }
}
