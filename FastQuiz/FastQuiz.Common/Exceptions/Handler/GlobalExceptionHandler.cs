using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FastQuiz.Common.Exceptions.Handler;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = 499;
            return true;
        }

        // 2. 根据异常类型决定返回逻辑
        var (statusCode, title) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "请先登录"),
            BadHttpRequestException badRequest => (badRequest.StatusCode, "请求参数格式错误"),
            BusinessException => (StatusCodes.Status400BadRequest, exception.Message),
            ResourceNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            PermissionException => (StatusCodes.Status403Forbidden, exception.Message),
            EntityArgumentException => (
                StatusCodes.Status422UnprocessableEntity,
                exception.Message
            ),
            _ => (StatusCodes.Status500InternalServerError, "服务器内部错误"),
        };

        if (statusCode >= 500)
        {
            logger.LogError(exception, "系统发生未处理异常: {Message}", exception.Message);
        }
        else
        {
            logger.LogInformation("请求处理异常 ({StatusCode}): {Message}", statusCode, exception.Message);
        }

        // 3. 设置响应内容
        httpContext.Response.StatusCode = statusCode;

        // 建议使用标准 RFC 7807 的 ProblemDetails 格式返回错误
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode >= 500 ? null : title,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            },
            cancellationToken
        );

        return true;
    }
}
