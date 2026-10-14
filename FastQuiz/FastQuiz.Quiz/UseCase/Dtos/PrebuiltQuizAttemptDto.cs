namespace FastQuiz.Quiz.UseCase.Dtos;

// No questions, answers or user data are returned by the inventory endpoint.
public record PrebuiltQuizAttemptDto(string Id, string QuizSnapshotId);
