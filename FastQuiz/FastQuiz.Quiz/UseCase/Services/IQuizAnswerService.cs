using FastQuiz.Quiz.UseCase.Commands;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizAnswerService
{
    Task AnswerQuestionAsync(
        string userId,
        string tenantId,
        string questionRecordId,
        IEnumerable<SetAnswerRecordCommand> commands
    );
}
