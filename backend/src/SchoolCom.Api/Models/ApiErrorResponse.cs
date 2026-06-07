namespace SchoolCom.Api.Models;

public record ApiErrorResponse(
    bool Success,
    string ErrorCode,
    string TranslationKey);

public static class AuthErrorCodes
{
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AccountDisabled = "AUTH_ACCOUNT_DISABLED";
    public const string InvalidOtp = "AUTH_INVALID_OTP";
    public const string UserNotFound = "AUTH_USER_NOT_FOUND";
    public const string Unauthorized = "AUTH_UNAUTHORIZED";
    public const string Forbidden = "AUTH_FORBIDDEN";
}

public static class AuthTranslationKeys
{
    public const string SessionExpired = "auth.sessionExpired";
    public const string InvalidCredentials = "auth.invalidCredentials";
    public const string AccountDisabled = "auth.accountDisabled";
    public const string InvalidOtp = "auth.invalidOtp";
    public const string UserNotFound = "auth.userNotFound";
    public const string Unauthorized = "auth.unauthorized";
    public const string Forbidden = "auth.forbidden";
}
