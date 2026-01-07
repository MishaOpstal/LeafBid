using System;

namespace LeafBidAPI.Helpers;

public static class TimeHelper
{
    private static readonly TimeZoneInfo AmsterdamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    public static DateTime GetAmsterdamTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AmsterdamTimeZone);
    }
}
