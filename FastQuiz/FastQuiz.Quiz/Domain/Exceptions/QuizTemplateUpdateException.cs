using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizTemplateUpdateException : BusinessException
{
    public QuizTemplateUpdateException() { }

    public QuizTemplateUpdateException(string? message)
        : base(message) { }

    public QuizTemplateUpdateException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
