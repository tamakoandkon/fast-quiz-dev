namespace FastQuiz.Question.Domain.Entities;

public enum MatchStrategy
{
    Exact = 0,
    Contains = 1,
    Regex = 2,
    Manual = 3,
}
