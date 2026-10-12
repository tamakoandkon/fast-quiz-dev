using FastQuiz.Common.Exceptions;

namespace FastQuiz.Question.Domain.Exceptions;

/// <summary>
/// 访问的题库不存在时抛出该异常
/// </summary>
public class QuestionGroupNotFoundException : ResourceNotFoundException
{
    public QuestionGroupNotFoundException() { }

    public QuestionGroupNotFoundException(string? message)
        : base(message) { }

    public QuestionGroupNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
