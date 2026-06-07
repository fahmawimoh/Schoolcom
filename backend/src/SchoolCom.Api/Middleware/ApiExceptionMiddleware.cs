using System.Net;
using System.Text.Json;
using FluentValidation;
using SchoolCom.Api.Helpers;
using SchoolCom.Api.Models;
using SchoolCom.Application.Common.Exceptions;

namespace SchoolCom.Api.Middleware;

public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed: {Errors}", ex.Errors);
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Validation failed",
                ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }).ToArray());
        }
        catch (AuthException ex)
        {
            logger.LogWarning("Auth error: {ErrorCode}", ex.ErrorCode);
            await WriteAuthErrorAsync(context, HttpStatusCode.Unauthorized, ex.ErrorCode, ex.TranslationKey);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found");
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Not found", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation");
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Invalid operation", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized");
            var (errorCode, translationKey) = AuthErrorResponseWriter.MapUnauthorizedMessage(ex.Message);
            await WriteAuthErrorAsync(context, HttpStatusCode.Unauthorized, errorCode, translationKey);
        }
    }

    private static async Task WriteAuthErrorAsync(
        HttpContext context,
        HttpStatusCode status,
        string errorCode,
        string translationKey)
    {
        if (context.Response.HasStarted) throw new InvalidOperationException("Response already started.");

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        var body = new ApiErrorResponse(false, errorCode, translationKey);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        HttpStatusCode status,
        string title,
        object detail)
    {
        if (context.Response.HasStarted) throw new InvalidOperationException("Response already started.");

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new
        {
            title,
            detail,
            status = (int)status,
            errors = detail is Array ? detail : new[] { new { message = detail.ToString() } }
        });

        await context.Response.WriteAsync(body);
    }
}
