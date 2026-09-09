using System.Text.Json;
using System.Text.Json.Serialization;
using FastQuiz.API.Setup;
using FastQuiz.Auth.Interface.Extensions;
using FastQuiz.Common.Setup;
using FastQuiz.Question.Interface;
using FastQuiz.Quiz.Interface.Setup;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;

namespace FastQuiz.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddResponseCompression();

        // 心跳检查
        builder.Services.AddHealthChecks();

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAuthorization();
        builder.Services.AddAuth(builder.Configuration, builder.Environment);
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
                    ?? throw new InvalidOperationException("跨域限制在生产环境下不能为空");

                options.AddDefaultPolicy(policy =>
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                );
            }
        });

        // 2. 生产环境验证 AllowedHosts
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

        var app = builder.Build();

        app.UseGlobalExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 请求压缩
        app.UseResponseCompression();
        app.UseRouting();

        app.UseCors();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthApi();
        app.MapQuestionGroupApi();
        app.MapQuizEndPoints();

        // 心跳检查
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

        // 只在开发环境自动迁移
        if (app.Environment.IsDevelopment())
        {
            app.ApplyAllMigrations();
        }

        app.Run();
    }
}
