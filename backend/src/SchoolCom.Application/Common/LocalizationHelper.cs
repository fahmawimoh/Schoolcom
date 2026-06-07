namespace SchoolCom.Application.Common;

public static class LocalizationHelper
{
    public static string Pick(string en, string ar, string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
            return en;

        return locale.StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? ar : en;
    }

    public static string FullName(string firstEn, string firstAr, string lastEn, string lastAr, string? locale) =>
        $"{Pick(firstEn, firstAr, locale)} {Pick(lastEn, lastAr, locale)}".Trim();
}
