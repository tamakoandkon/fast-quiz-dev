using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuestionSelectionRuleCreateException : BusinessException
{
    public QuestionSelectionRuleCreateException() { }

    public QuestionSelectionRuleCreateException(string? message)
        : base(message) { }

    public QuestionSelectionRuleCreateException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
