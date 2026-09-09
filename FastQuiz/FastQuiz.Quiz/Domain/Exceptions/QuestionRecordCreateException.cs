using FastQuiz.Common.Exceptions;

namespace FastQuiz.Quiz.Domain.Exceptions;

public class QuestionRecordCreateException : BusinessException
{
    public QuestionRecordCreateException() { }

    public QuestionRecordCreateException(string? message)
        : base(message) { }

    public QuestionRecordCreateException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
