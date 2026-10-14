using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics;
using System.Text.Json;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using FastQuiz.API.Setup;
using FastQuiz.Auth.Domain.Entities;
using FastQuiz.Auth.Domain.Repositories;
using FastQuiz.Auth.Domain.Services;
using FastQuiz.Auth.Infrastructure.Initializer;
using FastQuiz.Auth.Infrastructure.Services;
using FastQuiz.Quiz.UseCase.Services;
using Hangfire;
using Hangfire.Common;
using Hangfire.AspNetCore;
using Hangfire.States;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;

namespace FastQuiz.API.Test.Configuration;

public class ProductionConfigurationTests
{
    private static string RandomKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    private static JwtTokenSettings Settings(string key) => new() { Issuer = "test-issuer", Audience = "test-audience", Key = key };

    [Test]
    public void CommittedApplicationDefaultsContainNoCredentialsOrImplicitProvisioning()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));
        var config = json.RootElement;
        config.GetProperty("Jwt").TryGetProperty("Key", out _).ShouldBeFalse();
        config.GetProperty("BootstrapAdmin").GetProperty("Enabled").GetBoolean().ShouldBeFalse();
        config.GetProperty("BootstrapAdmin").TryGetProperty("Password", out _).ShouldBeFalse();
        config.GetProperty("Migrations").GetProperty("RunMigrationsOnStartup").GetBoolean().ShouldBeFalse();
        config.GetProperty("Hangfire").GetProperty("Type").GetString().ShouldBe("sqlite");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("short")]
    [TestCase("replace-with-a-private-random-key-at-least-32-bytes")]
    [TestCase("11111111-1111-4111-8111-111111111111")]
    public void MissingOrKnownUnsafeSigningKeyIsRejected(string? key)
    {
        var error = Should.Throw<InvalidOperationException>(() => Settings(key!).Validate());
        if (!string.IsNullOrEmpty(key))
            error.Message.ShouldNotContain(key);
    }

    [Test]
    public void AuthRegistrationRejectsMissingKeyBeforeStartingTheHost()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Auth:Type"] = "sqlite",
            ["Auth:ConnectionString"] = "Data Source=unused.db",
            ["Jwt:Issuer"] = "issuer",
            ["Jwt:Audience"] = "audience",
        }).Build();
        Should.Throw<InvalidOperationException>(() => new ServiceCollection().AddAuth(config))
            .Message.ShouldContain("Jwt:Key");
    }

    [Test]
    public void TokenGenerationAndValidationUseTheSameConfiguredSettings()
    {
        var settings = Settings(RandomKey());
        var generator = new TokenGenerator(Options.Create(settings));
        var token = generator.GenerateToken(new User("user", "username", null, "hash"));
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            ValidateLifetime = true,
        };
        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _).Identity!.IsAuthenticated.ShouldBeTrue();
        parameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RandomKey()));
        Should.Throw<SecurityTokenException>(() => new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("too-short")]
    [TestCase("more-than-twenty-characters")]
    public void BootstrapPasswordMustBeExplicitAndCompatibleWithLogin(string? password)
        => Should.Throw<InvalidOperationException>(() => new BootstrapAdminSettings { Password = password }.ValidateForCreation());

    [Test]
    public async Task DisabledBootstrapDoesNotResolveOrModifyAnyRepository()
    {
        using var services = new ServiceCollection().Configure<BootstrapAdminSettings>(_ => { }).BuildServiceProvider();
        await AuthDbSeedData.InitializeAsync(services);
    }

    [Test]
    public async Task MissingBootstrapPasswordFailsBeforeAnySeedWrites()
    {
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();
        var tenants = new Mock<ITenantRepository>();
        using var services = SeedServices(users, roles, tenants, new Mock<IPasswordHasher>(), null);
        await Should.ThrowAsync<InvalidOperationException>(() => AuthDbSeedData.InitializeAsync(services));
        users.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        roles.Verify(x => x.CreateAsync(It.IsAny<Role>()), Times.Never);
        tenants.Verify(x => x.CreateAsync(It.IsAny<Tenant>()), Times.Never);
    }

    [Test]
    public async Task BootstrapCreatesOnlyWithSuppliedPasswordAndNeverResetsExistingAdmin()
    {
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        User? saved = null;
        var users = new Mock<IUserRepository>();
        users.Setup(x => x.FindByIdAsync(User.DefaultAdminId)).ReturnsAsync(() => saved);
        users.Setup(x => x.CreateAsync(It.IsAny<User>())).Callback<User>(x => saved = x).Returns(Task.CompletedTask);
        var roles = new Mock<IRoleRepository>();
        roles.Setup(x => x.ExistsUserRoleAsync(User.DefaultAdminId, Role.DefaultAdminRoleId)).ReturnsAsync(true);
        var tenants = new Mock<ITenantRepository>();
        tenants.Setup(x => x.GetUserInTenantAsync(Tenant.DefaultTenantId, User.DefaultAdminId)).ReturnsAsync(true);
        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Hash(password)).Returns("private-hash");
        using (var services = SeedServices(users, roles, tenants, hasher, password))
            await AuthDbSeedData.InitializeAsync(services);
        saved!.Username.ShouldBe(User.DefaultAdminUsername);
        saved.PasswordHash.ShouldBe("private-hash");
        // Existing admin needs no bootstrap password; retry must not overwrite or rehash.
        using (var services = SeedServices(users, roles, tenants, hasher, null))
            await AuthDbSeedData.InitializeAsync(services);
        saved.PasswordHash.ShouldBe("private-hash");
        users.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        users.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        hasher.Verify(x => x.Hash(It.IsAny<string>()), Times.Once);
    }

    private static ServiceProvider SeedServices(Mock<IUserRepository> users, Mock<IRoleRepository> roles,
        Mock<ITenantRepository> tenants, Mock<IPasswordHasher> hasher, string? password)
        => new ServiceCollection()
            .Configure<BootstrapAdminSettings>(x => { x.Enabled = true; x.Password = password; })
            .AddSingleton(users.Object).AddSingleton(roles.Object).AddSingleton(tenants.Object)
            .AddSingleton(hasher.Object).BuildServiceProvider();

    [Test]
    public void MissingMigrationConfigurationDoesNotMigrate()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        app.ApplyAllMigrations(new ConfigurationBuilder().Build()).ShouldBeFalse();
    }

    [TestCase("Production")]
    [TestCase("Staging")]
    public void NonDevelopmentEnvironmentRejectsMemoryJobStorage(string name)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Hangfire:Type"] = "memory" }).Build();
        Should.Throw<InvalidOperationException>(() =>
            new ServiceCollection().AddHangfireServcie(config, new TestEnvironment(name)));
    }

    [Test]
    public void SqliteRejectsAConnectionStringInsteadOfSilentlyCreatingAnIncorrectFile()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Hangfire:Type"] = "sqlite", ["Hangfire:ConnectionString"] = "Data Source=wrong.db" }).Build();
        Should.Throw<InvalidOperationException>(() =>
            new ServiceCollection().AddHangfireServcie(config, new TestEnvironment("Production")));
    }

    [Test]
    public void DevelopmentMustExplicitlyOptInToMemoryStorage()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Hangfire:Type"] = "memory" }).Build();
        var services = new ServiceCollection().AddLogging().AddHangfireServcie(config, new TestEnvironment("Development"));
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<JobStorage>().GetType().Name.ShouldContain("InMemory");
    }

    [Test]
    [NonParallelizable] // Hangfire's global serializer configuration is process-wide.
    public async Task DefaultJobProviderPersistsAndExecutesDelayedJobsAcrossServiceProviderRecreation()
    {
        var directory = Path.Combine(Path.GetTempPath(), "fastquiz-hangfire-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "jobs.db");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Hangfire:ConnectionString"] = path }).Build();
        try
        {
            var executed = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var review = new Mock<IQuizAttemptReviewService>();
            review.Setup(x => x.ReviewQuizAttemptByAttemptIdAsync(It.IsAny<string>()))
                .Callback<string>(id => executed.TrySetResult(id)).Returns(Task.CompletedTask);
            string jobId;
            var services = new ServiceCollection().AddLogging().AddHangfireServcie(config, new TestEnvironment("Production")).AddSingleton(review.Object);
            using (var first = services.BuildServiceProvider())
            {
                var client = first.GetRequiredService<IBackgroundJobClient>();
                jobId = client.Schedule<IQuizAttemptReviewService>(x => x.ReviewQuizAttemptByAttemptIdAsync("persisted-attempt"), DateTimeOffset.UtcNow.AddSeconds(-1));
            }
            using (var second = services.BuildServiceProvider())
            using (var connection = second.GetRequiredService<JobStorage>().GetConnection())
            {
                var job = connection.GetJobData(jobId);
                job.ShouldNotBeNull();
                job.State.ShouldBe(ScheduledState.StateName);
                job.Job.Type.ShouldBe(typeof(IQuizAttemptReviewService));
                job.Job.Args[0].ShouldBe("persisted-attempt");
                using var server = new BackgroundJobServer(new BackgroundJobServerOptions
                {
                    WorkerCount = 1,
                    SchedulePollingInterval = TimeSpan.FromSeconds(1),
                    Activator = new AspNetCoreJobActivator(second.GetRequiredService<IServiceScopeFactory>()),
                }, second.GetRequiredService<JobStorage>());
                (await executed.Task.WaitAsync(TimeSpan.FromSeconds(25))).ShouldBe("persisted-attempt");
            }
            File.Exists(path).ShouldBeTrue();
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public async Task LegacyPersistedReviewJobEntryPointsRemainCallable()
    {
        var review = new Mock<IQuizAttemptReviewService>();
        var facade = new QuizAttemptService(Mock.Of<IQuizAttemptCreationService>(),
            Mock.Of<IQuizAttemptQueryService>(), Mock.Of<IQuestionRecordQueryService>(),
            Mock.Of<IQuizAnswerService>(), Mock.Of<IQuizAttemptLifecycleService>(), review.Object);
        // Deserialization of an existing stored job still resolves this public class/method.
        var job = Job.FromExpression<QuizAttemptService>(x => x.ReviewQuizAttemptByAttemptIdAsync("attempt"));
        job.Method.Name.ShouldBe(nameof(QuizAttemptService.ReviewQuizAttemptByAttemptIdAsync));
        await facade.ReviewQuizAttemptByAttemptIdAsync("attempt");
        await facade.ReviewQuizAttemptsByTemplateIdAsync("template");
        review.Verify(x => x.ReviewQuizAttemptByAttemptIdAsync("attempt"), Times.Once);
        review.Verify(x => x.ReviewQuizAttemptsByTemplateIdAsync("template"), Times.Once);
    }

    [Test]
    public async Task MigrationOnlyCommandInitializesAnIsolatedDatabaseThenExitsWithoutWorkers()
    {
        var directory = Path.Combine(Path.GetTempPath(), "fastquiz-init-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(8));
        var key = RandomKey();
        var authPath = Path.Combine(directory, "auth.db");
        var dotnet = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
            ?? Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "../../../dotnet"));
        var start = new ProcessStartInfo(dotnet)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add(typeof(Program).Assembly.Location);
        start.ArgumentList.Add("--contentRoot");
        start.ArgumentList.Add(AppContext.BaseDirectory);
        foreach (var pair in new Dictionary<string, string>
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["Jwt__Key"] = key,
            ["Jwt__Issuer"] = "test",
            ["Jwt__Audience"] = "test",
            ["AllowedHosts"] = "localhost",
            ["Cors__AllowedOrigins__0"] = "http://localhost",
            ["Migrations__RunMigrationsOnly"] = "true",
            ["Migrations__RunMigrationsOnStartup"] = "false",
            ["BootstrapAdmin__Enabled"] = "true",
            ["BootstrapAdmin__Password"] = password,
            ["Hangfire__Type"] = "sqlite",
            ["Hangfire__ConnectionString"] = Path.Combine(directory, "jobs.db"),
            ["Auth__Type"] = "sqlite",
            ["Auth__ConnectionString"] = "Data Source=" + authPath,
            ["Question__Type"] = "sqlite",
            ["Question__ConnectionString"] = "Data Source=" + Path.Combine(directory, "question.db"),
            ["Quiz__Type"] = "sqlite",
            ["Quiz__ConnectionString"] = "Data Source=" + Path.Combine(directory, "quiz.db"),
        }) start.Environment[pair.Key] = pair.Value;
        try
        {
            using var process = Process.Start(start)!;
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { process.Kill(true); throw; }
            var output = await stdout + await stderr;
            // Never include captured process output in an assertion failure with secrets.
            output.ShouldNotContain(password);
            output.ShouldNotContain(key);
            process.ExitCode.ShouldBe(0, "Migration-only process must exit successfully.");
            output.ShouldNotContain("Now listening on");
            output.ShouldNotContain("Starting Hangfire Server");
            File.Exists(Path.Combine(directory, "jobs.db")).ShouldBeFalse();

            await using var connection = new SqliteConnection("Data Source=" + authPath);
            await connection.OpenAsync();
            await using var query = connection.CreateCommand();
            query.CommandText = "SELECT PasswordHash FROM Users WHERE Id = $id";
            query.Parameters.AddWithValue("$id", User.DefaultAdminId);
            var hash = (string)(await query.ExecuteScalarAsync())!;
            new BCryptPasswordHasher().Verify(password, hash).ShouldBeTrue();
            hash.ShouldNotBe(password);
        }
        finally { SqliteConnection.ClearAllPools(); Directory.Delete(directory, true); }
    }

    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "FastQuiz.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
