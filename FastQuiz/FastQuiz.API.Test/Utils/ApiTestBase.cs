namespace FastQuiz.API.Test.Utils;

internal class ApiTestBase
{
    protected TestWebAppFactory Sut;

    [OneTimeSetUp]
    public void IniWebAppFactory()
    {
        Sut = new TestWebAppFactory();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
        Sut.Dispose();
    }

    protected HttpClient GetClient()
    {
        return Sut.CreateClient();
    }

    protected HttpClient GetAuthenticatiedClient(string userId)
    {
        var result = Sut.CreateClient();
        result.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        result.DefaultRequestHeaders.Add("X-Test-Authenticated", "true");
        return result;
    }

    protected HttpClient GetAuthenticatiedClient(string userId, string role)
    {
        var result = Sut.CreateClient();
        result.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        result.DefaultRequestHeaders.Add("X-Test-Authenticated", "true");
        result.DefaultRequestHeaders.Add("X-Test-Role", role);

        return result;
    }
}
