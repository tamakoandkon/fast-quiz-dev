using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastQuiz.Auth.Domain.Entities;

namespace FastQuiz.API.Test.Utils;

internal static class AuthHttpClientHelper
{
    internal static HttpClient CreateClient(this TestWebAppFactory webApp, string userId)
    {
        var result = webApp.CreateClient();
        result.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        result.DefaultRequestHeaders.Add("X-Test-Authenticated", "true");
        return result;
    }

    internal static HttpClient CreateSystemAdminClient(this TestWebAppFactory webApp)
    {
        var result = webApp.CreateClient();
        result.DefaultRequestHeaders.Add("X-Test-UserId", User.DefaultAdminId);
        result.DefaultRequestHeaders.Add("X-Test-Authenticated", "true");
        return result;
    }

    internal static HttpClient CreateClient(
        this TestWebAppFactory webApp,
        string userId,
        string role
    )
    {
        var result = webApp.CreateClient();
        result.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        result.DefaultRequestHeaders.Add("X-Test-Authenticated", "true");
        result.DefaultRequestHeaders.Add("X-Test-Role", role);
        return result;
    }

    private static readonly JsonSerializerOptions _defaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// 从 HttpResponseMessage 中读取并反序列化 JSON，支持字符串到枚举的自动转换
    /// </summary>
    public static async Task<T?> ReadFromJsonWithEnumAsync<T>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default
    )
    {
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_defaultOptions, cancellationToken);
    }

    /// <summary>
    /// 从 HttpResponseMessage 中读取并反序列化 JSON，支持字符串到枚举的自动转换
    /// 不抛出异常，通过返回值判断是否成功
    /// </summary>
    public static async Task<(
        T? Data,
        bool IsSuccess,
        HttpResponseMessage Response
    )> TryReadFromJsonWithEnumAsync<T>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default
    )
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var data = await response.Content.ReadFromJsonAsync<T>(
                    _defaultOptions,
                    cancellationToken
                );
                return (data, true, response);
            }
            catch (JsonException)
            {
                return (default, false, response);
            }
        }

        return (default, false, response);
    }

    /// <summary>
    /// 从 HttpResponseMessage 中读取并反序列化 JSON，支持自定义 JsonSerializerOptions
    /// </summary>
    public static async Task<T?> ReadFromJsonWithEnumAsync<T>(
        this HttpResponseMessage response,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default
    )
    {
        response.EnsureSuccessStatusCode();

        // 合并默认配置
        var mergedOptions = new JsonSerializerOptions(options);
        if (!mergedOptions.Converters.Any(c => c is JsonStringEnumConverter))
        {
            mergedOptions.Converters.Add(new JsonStringEnumConverter());
        }
        mergedOptions.PropertyNameCaseInsensitive = true;

        return await response.Content.ReadFromJsonAsync<T>(mergedOptions, cancellationToken);
    }
}
