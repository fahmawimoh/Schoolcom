namespace SchoolCom.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public string ErrorCode { get; }
    public string MessageEn { get; }
    public string MessageAr { get; }
    public string TranslationKey { get; }

    public ConflictException(
        string errorCode,
        string messageEn,
        string messageAr,
        string translationKey,
        string? message = null)
        : base(message ?? messageEn)
    {
        ErrorCode = errorCode;
        MessageEn = messageEn;
        MessageAr = messageAr;
        TranslationKey = translationKey;
    }
}
