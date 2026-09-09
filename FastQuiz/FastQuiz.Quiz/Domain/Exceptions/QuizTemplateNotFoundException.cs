using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizTemplateNotFoundException : ResourceNotFoundException
{
    public QuizTemplateNotFoundException() { }

    public QuizTemplateNotFoundException(string? message)
        : base(message) { }

    public QuizTemplateNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
