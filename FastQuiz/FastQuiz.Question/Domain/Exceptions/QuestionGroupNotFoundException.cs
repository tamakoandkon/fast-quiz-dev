using FastQuiz.Common.Exceptions;

namespace FastQuiz.Question.Domain.Exceptions;

public class QuestionGroupNotFoundException : ResourceNotFoundException
{
    public QuestionGroupNotFoundException() { }

    public QuestionGroupNotFoundException(string? message)
        : base(message) { }

    public QuestionGroupNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
