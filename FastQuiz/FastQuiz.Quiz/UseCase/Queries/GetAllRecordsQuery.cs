namespace FastQuiz.Quiz.UseCase.Queries;

/// <summary>
/// 查询指定考试的全部考试记录成绩。
/// </summary>
/// <param name="OnlyBest">是否只取每个考生最好的一次成绩</param>
public record GetAllRecordsQuery(
    bool OnlyBest = false,
    string? Cursor = null,
    int Count = 30,
    bool IsPrevious = false
);
