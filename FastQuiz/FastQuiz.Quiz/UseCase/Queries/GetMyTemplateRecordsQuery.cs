namespace FastQuiz.Quiz.UseCase.Queries;

/// <summary>
/// 查询当前登录用户在指定考试模板下的全部考试记录。
/// 与 <see cref="GetUserRecordsQuery"/> 的区别：不限定已批阅状态，
/// 「我的考试」页展开某个模板时需要看到进行中/待批阅的记录以便继续作答。
/// </summary>
/// <param name="Cursor">游标(记录 Id)，为空时取第一页</param>
/// <param name="Count">每页条数</param>
/// <param name="IsPrevious">是否取上一页(比 cursor 更新的记录)</param>
public record GetMyTemplateRecordsQuery(
    string? Cursor = null,
    int Count = 30,
    bool IsPrevious = false
);
