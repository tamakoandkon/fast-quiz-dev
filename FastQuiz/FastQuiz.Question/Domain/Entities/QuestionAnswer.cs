namespace FastQuiz.Question.Domain.Entities;

public class QuestionAnswer
{
    public string Id { get; private set; } = null!;
    public string QuestionId { get; private set; } = null!;
    public string MarkupContent { get; private set; } = null!;
    public bool IsCorrect { get; private set; } = false;
    public int SortOrder { get; private set; } = 0;
    public MatchStrategy MatchStrategy { get; private set; } = MatchStrategy.Exact;

    private QuestionAnswer() { }

    public QuestionAnswer(
        string id,
        string questionId,
        string markupContent,
        bool isCorrect,
        int sortOrder,
        MatchStrategy strategy
    )
    {
        Id = id;
        QuestionId = questionId;
        MarkupContent = markupContent;
        IsCorrect = isCorrect;
        SortOrder = sortOrder;
        MatchStrategy = strategy;
    }
}
