namespace FastQuiz.Quiz.Domain.Entities;

public enum QuizAttemptStatus
{
    Pending = 0, // 已分配，未开始
    InProgress = 1, // 进行中
    Submitted = 2, // 已提交
    Graded = 3, // 已批阅
}
