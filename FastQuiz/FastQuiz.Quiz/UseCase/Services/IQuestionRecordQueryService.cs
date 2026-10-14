using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;

namespace FastQuiz.Quiz.UseCase.Services;

public interface IQuestionRecordQueryService
{
    Task<
        IReadOnlyList<QuestionRecordWithoutAnswerDto>
    > GetAllQuestionWithoutAnswerDtoAsync(
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
}
