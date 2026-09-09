using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuizTemplateCreateException : BusinessException
{
    public QuizTemplateCreateException() { }

    public QuizTemplateCreateException(string? message)
        : base(message) { }

    public QuizTemplateCreateException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
