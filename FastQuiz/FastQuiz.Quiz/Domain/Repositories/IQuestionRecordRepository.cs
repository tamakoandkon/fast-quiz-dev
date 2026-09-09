using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Repositories;

public interface IQuestionRecordRepository
{
    Task CreateAsync(IEnumerable<QuestionRecord> records);
    Task<QuestionRecord?> FindQuestionRecordByIdAsync(string id);
    Task<IReadOnlyList<QuestionRecord>> GetAllByQuizAttempIdAsync(
        string quizAttemptId,
        string? cursor,
        int count
    );
    Task<IReadOnlyList<QuestionRecord>> GetAllByQuizAttempIdAsync(string quizAttemptId);
    Task UpdateAsync(QuestionRecord record);
    Task SetQuestionRecordsCorrectAsync(IEnumerable<string> idList, bool isCorrect);
    Task DeleteByQuizAttemptId(string quizAttemptId);
    Task CreateAnswerRecordAsync(IEnumerable<AnswerRecord> answerRecords);
    Task DeleteAnswerRecordByQuestionRecordIdAsync(string questionRecordId);
    Task DeleteAnswerRecordByQuestionRecordIdAsync(IEnumerable<string> questionRecordIdList);
}
