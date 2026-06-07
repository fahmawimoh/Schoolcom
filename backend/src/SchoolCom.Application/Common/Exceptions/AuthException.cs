namespace SchoolCom.Application.Common.Exceptions;

public class AuthException : Exception
{
    public string ErrorCode { get; }
    public string TranslationKey { get; }

    public AuthException(string errorCode, string translationKey, string? message = null)
        : base(message ?? translationKey)
    {
        ErrorCode = errorCode;
        TranslationKey = translationKey;
    }
}
