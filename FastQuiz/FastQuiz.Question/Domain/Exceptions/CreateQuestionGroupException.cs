using FastQuiz.Common.Exceptions;

namespace FastQuiz.Question.Domain.Exceptions;

public class CreateQuestionGroupException : BusinessException
{
    public CreateQuestionGroupException() { }

    public CreateQuestionGroupException(string? message)
        : base(message) { }

    public CreateQuestionGroupException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
