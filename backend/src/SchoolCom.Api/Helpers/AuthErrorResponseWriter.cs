using System.Text.Json;
using SchoolCom.Api.Models;

namespace SchoolCom.Api.Helpers;

public static class AuthErrorResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string translationKey)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new ApiErrorResponse(false, errorCode, translationKey);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    public static (string ErrorCode, string TranslationKey) MapUnauthorizedMessage(string message)
    {
        return message switch
        {
            "Invalid email or password" => (AuthErrorCodes.InvalidCredentials, AuthTranslationKeys.InvalidCredentials),
            "Account is disabled" => (AuthErrorCodes.AccountDisabled, AuthTranslationKeys.AccountDisabled),
            "Invalid or expired verification code" => (AuthErrorCodes.InvalidOtp, AuthTranslationKeys.InvalidOtp),
            "User not found" => (AuthErrorCodes.UserNotFound, AuthTranslationKeys.UserNotFound),
            "Admin access required for WhatsApp" => (AuthErrorCodes.Forbidden, AuthTranslationKeys.Forbidden),
            _ when message.Contains("Tenant not found", StringComparison.OrdinalIgnoreCase)
                => (AuthErrorCodes.Unauthorized, AuthTranslationKeys.Unauthorized),
            _ => (AuthErrorCodes.Unauthorized, AuthTranslationKeys.Unauthorized)
        };
    }
}
