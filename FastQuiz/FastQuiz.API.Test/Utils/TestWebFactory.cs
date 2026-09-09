using FastQuiz.API.Test.Utils.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FastQuiz.API.Test.Utils;

internal class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAuth().ConfigureQuestion().ConfigureQuiz();
    }
}
