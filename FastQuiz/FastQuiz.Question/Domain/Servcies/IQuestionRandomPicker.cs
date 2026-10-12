using FastQuiz.Question.Domain.Entities;
using Q = FastQuiz.Question.Domain.Entities.Question;

namespace FastQuiz.Question.Domain.Servcies;

/// <summary>
/// 题目随机抽取器
/// </summary>
public interface IQuestionRandomPicker
{
    /// <summary>
    /// 随机抽取题目
    /// </summary>
    /// <param name="groupId">题目分组id</param>
    /// <param name="questionType">题目类型</param>
    /// <param name="level">题目难度</param>
    /// <param name="count">抽取数量</param>
    /// <returns></returns>
    Task<IReadOnlyList<Q>> PickAsync(
        string groupId,
        QuestionType questionType,
        int level,
        int count
    );
}
