namespace FastQuiz.Question.Domain.Entities;

public class Question
{
    public string Id { get; private set; } = null!;
    public QuestionType Type { get; private set; } = QuestionType.SingleChoice;
    public int Level { get; private set; } = 0;
    public string MarkupContent { get; private set; } = null!;
    public string? Remark { get; private set; } = null;
    public string GroupId { get; private set; } = null!;
    public List<QuestionAnswer> Answers { get; set; } = null!;

    private Question() { }

    public Question(
        string id,
        QuestionType type,
        int level,
        string markupContent,
        string? remark,
        string groupId,
        IReadOnlyList<QuestionAnswer> answers
    )
    {
        Id = id;
        Type = type;
        Level = level;
        MarkupContent = markupContent;
        Remark = remark;
        GroupId = groupId;
        Answers = [.. answers];
    }

    public void SetAnswers(IEnumerable<QuestionAnswer> answers)
    {
        Answers = [.. answers];
    }

    public void SetLevel(int level)
    {
        Level = level;
    }

    public void SetMarkupContent(string markupContent)
    {
        MarkupContent = markupContent;
    }

    public void SetType(QuestionType type)
    {
        Type = type;
    }

    public void SetRemark(string? remark)
    {
        Remark = remark;
    }
}
