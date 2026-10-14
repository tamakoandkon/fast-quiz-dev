namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizAttemptReviewService
{
    Task ManualReviewQuestionRecordAsync(
        string tenantId,
        string myId,
        string questionRecordId,
        bool isCorrect
    );

    Task ReviewQuizAttemptByAttemptIdAsync(string quizAttemptId);

    Task ReviewQuizAttemptsByTemplateIdAsync(string templateId);
}
