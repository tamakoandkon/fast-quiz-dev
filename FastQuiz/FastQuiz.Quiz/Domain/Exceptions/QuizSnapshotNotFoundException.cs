using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizSnapshotNotFoundException : ResourceNotFoundException
{
    public QuizSnapshotNotFoundException() { }

    public QuizSnapshotNotFoundException(string? message)
        : base(message) { }

    public QuizSnapshotNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
