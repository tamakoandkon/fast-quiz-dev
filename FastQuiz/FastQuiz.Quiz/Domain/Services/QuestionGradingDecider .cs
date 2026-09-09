using System.Text.RegularExpressions;
using FastQuiz.Common.Exceptions;
using FastQuiz.Question.Domain.Entities;
using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Services;

public class QuestionGradingDecider : IQuestionGradingDecider
{
    public bool Decide(QuestionSnapshot question, QuestionRecord record)
    {
        if (question.Id != record.QuestionSnapshotId)
        {
            throw new BusinessException("答题记录和题目快照不匹配无法评分");
        }

        if (
            question.Type == QuestionType.MultipleChoice
            || question.Type == QuestionType.SingleChoice
        )
        {
            var correctList = question
                .AnswerSnapshots.Where(x => x.IsCorrect)
                .Select(x => x.Id)
                .Order()
                .ToList();

            var answerList = record.AnswerRecords.Select(x => x.Id).Order().ToList();

            return correctList.SequenceEqual(answerList);
        }
        else if (question.Type == QuestionType.FillIn)
        {
            var correctAnswer = question.AnswerSnapshots[0];
            var answer = record.AnswerRecords[0];
            return correctAnswer.Strategy switch
            {
                MatchStrategy.Exact => correctAnswer.MarkUpContent == answer.Value,
                MatchStrategy.Contains => answer.Value.Contains(correctAnswer.MarkUpContent),
                MatchStrategy.Regex => Regex.IsMatch(answer.Value, correctAnswer.MarkUpContent),
                _ => false,
            };
        }
        return false;
    }
}
