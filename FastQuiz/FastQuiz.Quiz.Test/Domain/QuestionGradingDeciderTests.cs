using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Services;
using Shouldly;

namespace FastQuiz.Quiz.Test.Domain;

internal class QuestionGradingDeciderTests
{
    [TestCase("(a+)+$", true)]
    [TestCase("[", false)]
    public void FillInRegex_FailsSafelyForTimeoutOrInvalidLegacyPattern(string pattern, bool timeout)
    {
        var question = new QuestionSnapshot { Id="q", Type=QuestionType.FillIn,
            AnswerSnapshots=[new() { MarkUpContent=pattern, Strategy=MatchStrategy.Regex }] };
        var record = new QuestionRecord { QuestionSnapshotId="q",
            AnswerRecords=[new() { Value=new string('a',10000)+"!" }] };
        var elapsed=System.Diagnostics.Stopwatch.StartNew();
        var error=Should.Throw<FastQuiz.Common.Exceptions.BusinessException>(()=>new QuestionGradingDecider().Decide(question,record));
        elapsed.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(2));
        error.Message.ShouldContain(timeout ? "超时" : "无效");
    }

    [TestCase(QuestionType.SingleChoice, "a", true)]
    [TestCase(QuestionType.SingleChoice, "b", false)]
    [TestCase(QuestionType.SingleChoice, "", false)]
    [TestCase(QuestionType.MultipleChoice, "a,c", true)]
    [TestCase(QuestionType.MultipleChoice, "c,a", true)]
    [TestCase(QuestionType.MultipleChoice, "a", false)]
    [TestCase(QuestionType.MultipleChoice, "a,b,c", false)]
    [TestCase(QuestionType.MultipleChoice, "", false)]
    public void ChoiceGradingComparesSelectedValuesRatherThanAnswerRecordIds(
        QuestionType type, string selected, bool expected)
    {
        var question = new QuestionSnapshot
        {
            Id = "question",
            Type = type,
            AnswerSnapshots = [
                new() { Id = "a", IsCorrect = true },
                new() { Id = "b", IsCorrect = false },
                new() { Id = "c", IsCorrect = type == QuestionType.MultipleChoice },
            ],
        };
        var record = new QuestionRecord
        {
            QuestionSnapshotId = question.Id,
            AnswerRecords = selected.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(value => new AnswerRecord { Id = Guid.NewGuid().ToString(), Value = value }).ToList(),
        };
        new QuestionGradingDecider().Decide(question, record).ShouldBe(expected);
    }
}
