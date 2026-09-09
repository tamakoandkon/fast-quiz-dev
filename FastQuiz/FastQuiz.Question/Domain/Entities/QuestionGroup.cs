namespace FastQuiz.Question.Domain.Entities;

public class QuestionGroup
{
    public string Id { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Remark { get; private set; } = null;
    public string OwnerId { get; private set; } = null!;

    private QuestionGroup() { }

    public QuestionGroup(string id, string ownerId, string name, string? remark = null)
    {
        Id = id;
        OwnerId = ownerId;
        Name = name;
        Remark = remark;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetRemark(string? remark)
    {
        Remark = remark;
    }
}
