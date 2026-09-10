using Xunit;

namespace GymTrackerMobile.UI.AutomationTests.Infrastructure;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AndroidFactAttribute : FactAttribute
{
    public AndroidFactAttribute()
    {
        var apk = Environment.GetEnvironmentVariable("GYMTRACKER_APK_PATH");
        if (string.IsNullOrWhiteSpace(apk) || !File.Exists(apk))
        {
            Skip = "Configure APPIUM_SERVER_URL and GYMTRACKER_APK_PATH to run Android UI automation.";
        }
    }
}
