namespace GMMS.Mobile.Configuration;

public static class ApiSettings
{
    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5161"
            : "http://localhost:5161";

    public static TimeSpan RequestTimeout { get; } = TimeSpan.FromSeconds(30);
}