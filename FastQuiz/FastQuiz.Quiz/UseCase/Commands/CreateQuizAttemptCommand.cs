namespace FastQuiz.Quiz.UseCase.Commands;

public record CreateQuizAttemptCommand(string QuizSnapshotId, int Count);
