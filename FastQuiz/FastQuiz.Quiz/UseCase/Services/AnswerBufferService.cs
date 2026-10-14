using FastQuiz.Common.Utils;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Repositories;

namespace FastQuiz.Quiz.UseCase.Services;

/// <summary>
/// 保留原服务名以兼容现有依赖，但答案不再停留在易失内存中。
/// 每次提交都会在请求内以数据库事务完成“删除旧答案 + 写入新答案”。
/// </summary>
public class AnswerBufferService(
    IQuestionRecordRepository questionRecordRepository,
    IQuizUnitOfWork unitOfWork,
    IIdGenerator idGenerator
) : IAnswerBufferService
{
    public Task SetAnswersAsync(
        PendingAnswer answer,
        CancellationToken cancellationToken = default
    ) =>
        unitOfWork.ExecuteInTransactionAsync(
            async _ =>
            {
                await questionRecordRepository
                    .DeleteAnswerRecordByQuestionRecordIdAsync(answer.QuestionRecordId)
                    .ConfigureAwait(false);

                if (answer.Values.Count == 0)
                {
                    return;
                }

                var answerRecords = answer.Values
                    .Distinct()
                    .Select(value => new AnswerRecord
                    {
                        Id = idGenerator.GenerateUlId(),
                        QuestionRecordId = answer.QuestionRecordId,
                        QuizAttemptId = answer.QuizAttemptId,
                        Value = value,
                    })
                    .ToList();

                await questionRecordRepository
                    .CreateAnswerRecordAsync(answerRecords)
                    .ConfigureAwait(false);
            },
            cancellationToken
        );
}
