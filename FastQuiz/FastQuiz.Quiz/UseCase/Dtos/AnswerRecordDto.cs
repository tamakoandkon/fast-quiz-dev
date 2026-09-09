namespace FastQuiz.Quiz.UseCase.Dtos;

public record AnswerRecordDto(
    string Id,
    string QuizAttemptId,
    string QuestionRecordId,
    string Value
);
