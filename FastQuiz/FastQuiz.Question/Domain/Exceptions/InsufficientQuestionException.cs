using FastQuiz.Common.Exceptions;

namespace FastQuiz.Question.Domain.Exceptions;

public class InsufficientQuestionException : BusinessException
{
    public InsufficientQuestionException() { }

    public InsufficientQuestionException(string? message)
        : base(message) { }

    public InsufficientQuestionException(string? message, Exception? innerException)
        : base(message, innerException) { }

    public InsufficientQuestionException(int actualCount, int requiredCount)
        : base($"题目数量不足：实际 {actualCount} 道，需要 {requiredCount} 道") { }
}
