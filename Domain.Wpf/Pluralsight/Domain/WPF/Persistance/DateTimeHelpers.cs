using System;

namespace Pluralsight.Domain.WPF.Persistance;

internal static class DateTimeHelpers
{
    public const string Iso8601Format = "yyyy-MM-ddTHH:mm:ssZ";

    public static DateTimeOffset ParseWithDefault(this string value, DateTimeOffset defaultValue)
    {
        if (DateTimeOffset.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }

    public static string ToDatabaseFormat(this DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
