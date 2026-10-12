using static System.Guid;

namespace FastQuiz.Test.Common;

/// <summary>
/// 测试用标识生成工具。
/// 统一约定：测试数据不使用固定字符串字面量作为 ID，一律使用随机 GUID 或“随机前缀 + 可排序序号”，
/// 以避免测试之间互相污染，并保证分页/排序类断言依赖的 ID 顺序稳定可预测。
/// </summary>
public static class TestId
{
    /// <summary>
    /// 生成随机 ID（GUID 字符串）。
    /// </summary>
    public static string New() => NewGuid().ToString();

    /// <summary>
    /// 生成随机前缀（无分隔符），用于构造可排序 ID 或名称前缀，避免与其它测试数据冲突。
    /// </summary>
    public static string RandomToken() => NewGuid().ToString("N");

    /// <summary>
    /// 生成可排序的测试 ID：随机前缀 + 零填充序号，
    /// 保证按字符串序与序号一致，同时不与其他测试数据冲突。
    /// </summary>
    public static string Sortable(int index, string? prefix = null) =>
        $"{prefix ?? RandomToken()}-{index:000}";
}
