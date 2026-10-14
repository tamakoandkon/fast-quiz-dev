using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;
using FastQuiz.Quiz.Domain.Services;
using Shouldly;

namespace FastQuiz.Quiz.Test.Domain;

internal class QuestionGradingDeciderTests
{
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
