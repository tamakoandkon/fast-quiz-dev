using FastQuiz.Common.Exceptions;

namespace FastQuiz.Question.Domain.Exceptions;

public class UpdateQuestionException : BusinessException
{
    public UpdateQuestionException() { }

    public UpdateQuestionException(string? message)
        : base(message) { }

    public UpdateQuestionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
