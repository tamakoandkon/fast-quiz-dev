using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuizAtteptService
{
    Task<QuizAttempDto> CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId
    );
    Task CreateQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        IEnumerable<CreateQuizAttemptCommand> commands
    );
    Task<IReadOnlyList<QuizAttempDto>> GetAllQuizAttemptAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllQuizAttemptQuery query
    );
    /// <summary>
    /// 查看指定考试的全部考试记录成绩。仅拥有 quiz 模块权限(QuizAttempt)的用户可用。
    /// query.OnlyBest 为 true 时按考生只返回最好的一次成绩。
    /// </summary>
    Task<IReadOnlyList<QuizAttempDto>> GetAllRecordsByTemplateIdAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetAllRecordsQuery query
    );
    Task<IReadOnlyList<QuizAttempDto>> GetMyQuizAttemptAsync(
        string userId,
        string tenantId,
        GetMyQuizAttemptQuery query
    );
    /// <summary>
    /// 查看当前登录用户在指定考试模板下的全部考试记录(包含未开始/进行中/待批阅/已批阅)。
    /// 用于「我的考试」页展开某个模板时列出该场考试的历次记录。
    /// </summary>
    Task<IReadOnlyList<QuizAttempDto>> GetMyTemplateRecordsAsync(
        string userId,
        string tenantId,
        string quizTemplateId,
        GetMyTemplateRecordsQuery query
    );
    /// <summary>
    /// 查看指定考生(userId)的考试记录。本人(操作者即该考生)或拥有 quiz 模块权限(QuizAttempt)的用户可用。
    /// 仅返回已完成的考试(已批阅)。
    /// query.TemplateId 为空时不限定考试；query.OnlyBest 为 true 时每场考试只返回该考生最好的一次成绩。
    /// </summary>
    Task<IReadOnlyList<QuizAttempDto>> GetUserRecordsAsync(
        string operatorId,
        string tenantId,
        string userId,
        GetUserRecordsQuery query
    );
    Task<QuizAttempDto> GetQuizAttemptByIdAsync(
        string userId,
        string tenantId,
        string quizAttemptId
    );
    Task AssignQuizAttemptAsync(string userId, string tenantId, string quizAttemptId);
    Task UpdateQuizAttemptStatusAsync(
        string userId,
        string tenantId,
        string quizAttemptId,
        QuizAttemptStatus status
    );
    Task AnswerQuestionAsync(
        string userId,
        string tenantId,
        string questionRecordId,
        IEnumerable<SetAnswerRecordCommand> commands
    );
    Task<IReadOnlyList<QuestionRecordWithoutAnswerDto>> GetAllQuestionWithoutAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    );
    Task<IReadOnlyList<QuestionRecordWithAnswerDto>> GetAllQuestionWithAnswerDtoAsync(
        string userId,
        string tenantId,
        string attemptId,
        GetAllQuestionRecordDtoQuery query
    );
    Task ManualReviewQuestionRecordAsync(
        string tenantId,
        string myId,
        string questionRecordId,
        bool isCorrect
    );
}
