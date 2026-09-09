using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IAnswerBufferService
{
    void AddAnswer(PendingAnswer answer);
    Task FlushAsync();
    Task CheckAndFlushAsync();
}
