using FastQuiz.Common.Utils;
using FastQuiz.Quiz.Domain.Repositories;
using FastQuiz.Quiz.UseCase.Commands;
using FastQuiz.Quiz.UseCase.Dtos;
using FastQuiz.Quiz.UseCase.Queries;
using FluentValidation;

namespace FastQuiz.Quiz.UseCase.Services;

public class QuizService(
    IQuizTemplateRepository _quizTemplateRepository,
    IQuestionSelectionRuleRepository _questionSelectionRuleRepository,
    IIdGenerator _idGenerator,
    IValidator<CreateQuizTemplateCommand> _createQuizTemplateCommandValidator
) : IQuizService
{
    /// <summary>
    /// 根据template中的question selection rule 抽取题目，然后创建对应的题目快照和quiz snapshot
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="quizTemplateId"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task GenerateQuizSnapshotAsync(
        string userId,
        string quizTemplateId,
        CreateQuizSnapshotCommand command
    )
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<QuizSnapshotDto>> GetAllQuizSnapshotAsync(
        string userId,
        string quiztemplateId,
        GetAllQuizSnapshotQuery query
    )
    {
        throw new NotImplementedException();
    }

    public Task DeleteQuizSnapshotByIdAsync(string userId, string id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 生成未分配的用户试卷，同时生成答题记录
    /// </summary>
    /// <param name="user"></param>
    /// <param name="quizTemplateId"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task CreateQuizAttemptAsync(
        string user,
        string quizTemplateId,
        CreateQuizAttemptCommand command
    )
    {
        throw new NotImplementedException();
    }

    public Task<QuizAttempSecretDto> GetQuizAttemptByIdAsync(string userId, string quizAttemptId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 删除之前的答题记录，添加新的答题记录
    /// </summary>
    /// <param name="questionRecordId"></param>
    /// <param name="commands"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task SetAnswerRecordAsync(
        string questionRecordId,
        IReadOnlyList<SetAnswerRecordCommand> commands
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 将未分配的试卷分配给当前用户，需要检查当前用户是否已经存在未结束的试卷，是否有资格继续分配试卷
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="quizTemplateId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<QuizAttempSecretDto> AssignQuizAttemptAsync(string userId, string quizTemplateId)
    {
        throw new NotImplementedException();
    }
}
