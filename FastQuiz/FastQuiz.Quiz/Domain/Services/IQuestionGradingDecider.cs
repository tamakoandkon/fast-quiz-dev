using FastQuiz.Quiz.Domain.Entities;

namespace FastQuiz.Quiz.Domain.Services;

public interface IQuestionGradingDecider
{
    bool Decide(QuestionSnapshot question, QuestionRecord record);
}
