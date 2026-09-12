namespace GymTrackerMobile.UI.AutomationTests.Infrastructure;

public sealed record UiTestSettings(Uri ServerUrl, string DeviceName, string ApkPath)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApkPath) && File.Exists(ApkPath);

    public static UiTestSettings Load()
    {
        var server = Environment.GetEnvironmentVariable("APPIUM_SERVER_URL") ?? "http://127.0.0.1:4723";
        var device = Environment.GetEnvironmentVariable("ANDROID_DEVICE_NAME") ?? "GymTrackerApi35";
        var apk = Environment.GetEnvironmentVariable("GYMTRACKER_APK_PATH") ?? string.Empty;
        return new UiTestSettings(new Uri(server), device, apk);
    }
}
