using FastQuiz.API.Test.Utils;

namespace FastQuiz.API.Test.Examples;

internal class ApiExampleTests : ApiTestBase
{
    /// <summary>
    /// api测试方法的命名规则如下
    /// 在编写一个方法的测试的时候，应该先编写正常情况，再逐步编写异常情况
    /// </summary>
    /// <returns></returns>
    [Test]
    public Task MethodAction_WhenCondition_ShouldResult()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 测试方法的一个例子
    /// </summary>
    /// <returns></returns>
    [Test]
    public Task PostLogin_WhenUsernameDoesNotExist_ShouldReturnBadRequest()
    {
        return Task.CompletedTask;
    }
}
