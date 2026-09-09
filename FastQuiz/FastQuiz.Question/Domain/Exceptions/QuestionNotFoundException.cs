using FastQuiz.Common.Exceptions;

namespace FastQuiz.Question.Domain.Exceptions;

public class QuestionNotFoundException : ResourceNotFoundException
{
    public QuestionNotFoundException() { }

    public QuestionNotFoundException(string? message)
        : base(message) { }

    public QuestionNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
