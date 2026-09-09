using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Test.Infrastructure.Repositories.Utils;

public class AnswerRecordBuilder
{
    private string _id = Guid.NewGuid().ToString();
    private string _quizAttemptId = Guid.NewGuid().ToString();
    private string _questionRecordId = Guid.NewGuid().ToString();
    private string _value = "Test Answer";

    public AnswerRecordBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public AnswerRecordBuilder WithQuizAttemptId(string quizAttemptId)
    {
        _quizAttemptId = quizAttemptId;
        return this;
    }

    public AnswerRecordBuilder WithQuestionRecordId(string questionRecordId)
    {
        _questionRecordId = questionRecordId;
        return this;
    }

    public AnswerRecordBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }

    public AnswerRecord Build()
    {
        return new AnswerRecord
        {
            Id = _id,
            QuizAttemptId = _quizAttemptId,
            QuestionRecordId = _questionRecordId,
            Value = _value,
        };
    }

    public List<AnswerRecord> BuildMany(
        int count,
        Func<AnswerRecordBuilder, int, AnswerRecordBuilder>? configure = null
    )
    {
        var results = new List<AnswerRecord>();
        for (int i = 0; i < count; i++)
        {
            var builder = new AnswerRecordBuilder();
            if (configure != null)
            {
                builder = configure(builder, i);
            }
            results.Add(builder.Build());
        }
        return results;
    }

    public static AnswerRecordBuilder New()
    {
        return new AnswerRecordBuilder();
    }
}
