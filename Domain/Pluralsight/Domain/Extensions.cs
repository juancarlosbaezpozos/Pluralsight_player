using System;
using System.Collections.Generic;

namespace Pluralsight.Domain;

public static class Extensions
{
    private static readonly Random Rand = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int num = list.Count;
        while (num > 1)
        {
            num--;
            int index = Rand.Next(num + 1);
            T value = list[index];
            list[index] = list[num];
            list[num] = value;
        }
    }

    public static string ToIso8601Format(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.DateTime.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffzzz");
    }

    public static long GetUnixTimestampInMicroseconds(this DateTimeOffset updatedDate)
    {
        return (updatedDate - Epoch()).Ticks / 10;
    }

    public static DateTimeOffset FromUnixTimestampInMicroseconds(this long timestamp)
    {
        return Epoch().AddTicks(timestamp * 10);
    }

    public static DateTimeOffset Epoch()
    {
        return new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
