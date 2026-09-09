using FastQuiz.Common.Exceptions;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Exceptions;
using FastQuiz.Quiz.Domain.Repositories;

namespace FastQuiz.Quiz.Domain.Services;

/// <summary>
/// 试卷阅卷领域服务。以 quizAttemptId 为唯一入口驱动自动判分与整卷收尾阅卷，
/// 具体“每题对错判定”委托给 <see cref="IQuestionGradingDecider"/>。
/// </summary>
public class QuizReviewService(
    IQuizAttemptRepository _quizAttemptRepository,
    IQuizTemplateRepository _quizTemplateRepository,
    IQuestionRecordRepository _questionRecordRepository,
    IQuestionSnapshotRepository _questionSnapshotRepository,
    IQuestionSelectionRuleRepository _questionSelectionRuleRepository,
    IQuestionGradingDecider _questionGradingDecider
) : IQuizReviewService
{
    public async Task ReviewAttemptAsync(string quizAttemptId)
    {
        // 0) 获取试卷
        var attempt =
            await _quizAttemptRepository.FindByIdAsync(quizAttemptId)
            ?? throw new ResourceNotFoundException("attempt 不存在");

        if (attempt.Status == QuizAttemptStatus.Graded)
        {
            // 已阅，无需重复判分
            return;
        }

        // 1) 获取模板
        var template =
            await _quizTemplateRepository.FindByIdAsync(attempt.QuizTemplateId)
            ?? throw new QuizTemplateNotFoundException();

        // 2) 获取抽题规则
        var rules = await _questionSelectionRuleRepository.GetByQuizTemplateIdAsync(template.Id);

        // 3) 获取答题记录和题目快照列表
        var joined = await BuildJoinedListAsync(quizAttemptId);

        // 4) 找出需要人工判卷(Manual)的题目
        var manualRecordIds = CollectManualRecordIds(joined);

        // 5) 先只对可自动判的题逐题写 IsCorrect；Manual 题保留原值交人工判定。
        var score = await JudgeAndPersistAsync(
            joined.Where(x => !manualRecordIds.Contains(x.Record.Id)).ToList(),
            rules
        );

        if (manualRecordIds.Count == 0)
        {
            // 整卷都能自动判：判完收尾，完成阅卷
            attempt.Score = score;
            attempt.Status = QuizAttemptStatus.Graded;
            await _quizAttemptRepository.UpdateAsync(attempt);
            return;
        }

        // 存在 Manual 题：整卷判定为“需要人工阅卷”，不自动完成阅卷；状态仍停留在 Submitted 待人工，
        // 最终得分由 CompleteReviewAsync 在全卷人工判定后统一汇总，此处不落半截分。
    }

    public async Task CompleteReviewAsync(string quizAttemptId)
    {
        var attempt =
            await _quizAttemptRepository.FindByIdAsync(quizAttemptId)
            ?? throw new ResourceNotFoundException("attempt 不存在");

        if (attempt.Status == QuizAttemptStatus.Graded)
        {
            return;
        }

        var rules = await _questionSelectionRuleRepository.GetByQuizTemplateIdAsync(
            attempt.QuizTemplateId
        );
        var joined = await BuildJoinedListAsync(quizAttemptId);

        // 不再重判(re-decode)：得分按已定判的每题 IsCorrect × 对应规则分数求和
        attempt.Score = joined.Sum(x =>
            x.Record.IsCorrect
                ? FindRuleScore(rules, x.Question)
                : 0
        );
        attempt.Status = QuizAttemptStatus.Graded;
        await _quizAttemptRepository.UpdateAsync(attempt);
    }

    public async Task<bool> HasManualQuestionAsync(string quizAttemptId)
    {
        var joined = await BuildJoinedListAsync(quizAttemptId);
        return CollectManualRecordIds(joined).Count > 0;
    }

    /// <summary>
    /// 取答题记录并 join 出题目快照；同时对记录自身可能尚未回填的答案集合不做假设，
    /// 仅凭仓库返回为准。
    /// </summary>
    private async Task<List<(QuestionRecord Record, QuestionSnapshot Question)>> BuildJoinedListAsync(
        string quizAttemptId
    )
    {
        var records = await _questionRecordRepository.GetAllByQuizAttempIdAsync(quizAttemptId);
        var questions = await _questionSnapshotRepository.FindByIdAsync(
            records.Select(x => x.QuestionSnapshotId)
        );

        return records
            .Join(
                questions,
                record => record.QuestionSnapshotId,
                question => question.Id,
                (record, question) => (Record: record, Question: question)
            )
            .ToList();
    }

    /// <summary>需要人工判断的答题记录 Id：题目快照含有 Manual 匹配策略的填空题。</summary>
    private static List<string> CollectManualRecordIds(
        List<(QuestionRecord Record, QuestionSnapshot Question)> joined
    )
    {
        return joined
            .Where(x => IsManual(x.Question))
            .Select(x => x.Record.Id)
            .ToList();
    }

    private static bool IsManual(QuestionSnapshot question)
    {
        return question.AnswerSnapshots.Any(s => s.Strategy == MatchStrategy.Manual);
    }

    private static int FindRuleScore(
        IReadOnlyList<QuestionSelectionRule> rules,
        QuestionSnapshot question
    )
    {
        return rules
            .FirstOrDefault(x =>
                x.GroupId == question.GroupId
                && x.Level == question.Level
                && x.Type == question.Type
            )
            ?.Score
            ?? throw new QuestionSelectionRuleNotFoundException();
    }

    /// <summary>逐题判分并写回 IsCorrect，返回这部分题目的总分。</summary>
    private async Task<int> JudgeAndPersistAsync(
        List<(QuestionRecord Record, QuestionSnapshot Question)> joined,
        IReadOnlyList<QuestionSelectionRule> rules
    )
    {
        var correctIdList = new List<string>();
        var unCorrectIdList = new List<string>();
        int totalScore = 0;

        for (int i = 0; i < joined.Count; i++)
        {
            var item = joined[i];
            var score = FindRuleScore(rules, item.Question);

            // 未作答(填空题无答案)按答错处理，避免越界/误判
            var isCorrect = item.Question.Type != QuestionType.FillIn
                || item.Record.AnswerRecords.Count > 0
                    ? _questionGradingDecider.Decide(item.Question, item.Record)
                    : false;

            if (isCorrect)
            {
                correctIdList.Add(item.Record.Id);
                totalScore += score;
            }
            else
            {
                unCorrectIdList.Add(item.Record.Id);
            }
        }

        if (unCorrectIdList.Count != 0)
        {
            await _questionRecordRepository.SetQuestionRecordsCorrectAsync(unCorrectIdList, false);
        }
        if (correctIdList.Count != 0)
        {
            await _questionRecordRepository.SetQuestionRecordsCorrectAsync(correctIdList, true);
        }

        return totalScore;
    }
}
