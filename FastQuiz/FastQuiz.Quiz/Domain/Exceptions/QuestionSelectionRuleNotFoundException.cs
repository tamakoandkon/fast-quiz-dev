using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuestionSelectionRuleNotFoundException : ResourceNotFoundException
{
    public QuestionSelectionRuleNotFoundException() { }

    public QuestionSelectionRuleNotFoundException(string? message)
        : base(message) { }

    public QuestionSelectionRuleNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
