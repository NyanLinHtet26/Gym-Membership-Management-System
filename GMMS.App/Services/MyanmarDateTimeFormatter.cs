namespace GMMS.App.Services;

public static class MyanmarDateTimeFormatter
{
    private static readonly TimeZoneInfo MmtTimeZone;

    static MyanmarDateTimeFormatter()
    {
        try
        {
            MmtTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                MmtTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yangon");
            }
            catch
            {
                MmtTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                    "Asia/Yangon_custom",
                    TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(30)),
                    "(UTC+06:30) Myanmar",
                    "Myanmar Standard Time");
            }
        }
    }

    public static DateTime ToMyanmarTime(this DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, MmtTimeZone);
    }

    public static DateTimeOffset ToMyanmarTime(this DateTimeOffset dateTimeOffset)
        => TimeZoneInfo.ConvertTime(dateTimeOffset, MmtTimeZone);

    public static DateOnly TodayMyanmarDateOnly
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MmtTimeZone));

    public static DateTime TodayMyanmarDateTime
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MmtTimeZone);

    public static string FormatMmtDateTime(this DateTime utc, string format = "yyyy-MM-dd HH:mm")
        => utc.ToMyanmarTime().ToString(format);

    public static string FormatMmtDateTime(this DateTime? utc, string format = "yyyy-MM-dd HH:mm")
        => utc.HasValue ? utc.Value.ToMyanmarTime().ToString(format) : "-";

    public static string FormatMmtLongDate(this DateTime utc)
        => utc.ToMyanmarTime().ToString("dddd, MMMM d, yyyy");

    public static string FormatMmtLongDateToday()
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MmtTimeZone).ToString("dddd, MMMM d, yyyy");
}
