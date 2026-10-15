using System.Text.Json;
using System.Text.Json.Serialization;
using FastQuiz.API.Setup;
using FastQuiz.Auth.Interface.Extensions;
using FastQuiz.Auth.Infrastructure.Initializer;
using FastQuiz.Common.Setup;
using FastQuiz.Question.Interface;
using FastQuiz.Quiz.Interface.Setup;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using NLog.Extensions.Logging;
using NLog.Web;

namespace FastQuiz.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddNLog();
        });

        var builder = WebApplication.CreateBuilder(args);

        // 配置 NLog
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddResponseCompression();

        // 健康检查
        builder.Services.AddHealthCheckServices(builder.Configuration);

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthorization();
        builder.Services.AddHangfireServcie(builder.Configuration, builder.Environment);

        builder.Services.AddAuth(builder.Configuration);
        builder.Services.AddQuestion(builder.Configuration);
        builder.Services.AddCommon().AddGlobalExceptionHandler();
        builder.Services.AddQuiz(builder.Configuration);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            if (builder.Environment.IsDevelopment())
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
                );
            }
            else
            {
                var allowedOrigins =
                    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? throw new InvalidOperationException("生产环境必须配置允许的跨域来源");

                options.AddDefaultPolicy(policy =>
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                );
            }
        });

        // 2. 生产环境校验 AllowedHosts
        if (!builder.Environment.IsDevelopment())
        {
            var allowedHosts = builder.Configuration["AllowedHosts"];
            if (string.IsNullOrEmpty(allowedHosts) || allowedHosts == "*")
            {
                throw new InvalidOperationException(
                    "AllowedHosts must be configured with specific domains in production."
                );
            }
        }

        await using var app = builder.Build();
        if (app.ApplyAllMigrations(app.Configuration))
        {
            await AuthDbSeedData.InitializeAsync(app.Services);
            return;
        }

        app.UseGlobalExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 响应压缩
        app.UseResponseCompression();
        app.UseRouting();

        app.UseCors();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        // Background workers do not need a public task dashboard.
        // In particular, localhost reverse proxies must not imply administrator access.

        app.MapAuthEndPoints();
        app.MapQuestionGroupEndPoints();
        app.MapQuizEndPoints();

        // 健康检查
        app.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = new
                    {
                        status = report.Status.ToString(),
                        timestamp = DateTimeOffset.UtcNow,
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                        }),
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                },
            }
        );

        try
        {
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            // 启动阶段异常也要落盘，避免只留在控制台
            NLog.LogManager.GetCurrentClassLogger()
                .Fatal(ex, "应用程序异常终止: {Message}", ex.Message);
            throw;
        }
        finally
        {
            // 刷新异步日志，防止缓冲区内容丢失
            NLog.LogManager.Shutdown();
        }
    }
}
