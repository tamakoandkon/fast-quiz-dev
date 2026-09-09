using FastQuiz.Common.Exceptions;

namespace FastQuiz.Question.Domain.Exceptions;

public class CreateQuestionException : BusinessException
{
    public CreateQuestionException() { }

    public CreateQuestionException(string? message)
        : base(message) { }

    public CreateQuestionException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
