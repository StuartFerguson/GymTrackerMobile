using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Infrastructure;

public sealed class AndroidDriverFixture : IAsyncLifetime
{
    private string? _failureDirectory;

    public UiTestSettings Settings { get; } = UiTestSettings.Load();
    public AndroidDriver? Driver { get; private set; }

    public Task InitializeAsync()
    {
        if (!Settings.IsConfigured)
        {
            return Task.CompletedTask;
        }

        var options = new AppiumOptions
        {
            PlatformName = "Android",
            AutomationName = "UiAutomator2",
            DeviceName = Settings.DeviceName,
            App = Settings.ApkPath,
        };
        options.AddAdditionalAppiumOption("appium:noReset", false);
        options.AddAdditionalAppiumOption("appium:fullReset", false);
        options.AddAdditionalAppiumOption("appium:autoGrantPermissions", true);
        options.AddAdditionalAppiumOption("appium:optionalIntentArguments", "-e gymtracker.uiTestMode true");
        options.AddAdditionalAppiumOption("appium:adbExecTimeout", 120_000);
        options.AddAdditionalAppiumOption("appium:uiautomator2ServerInstallTimeout", 120_000);
        options.AddAdditionalAppiumOption("appium:uiautomator2ServerLaunchTimeout", 120_000);
        Driver = new AndroidDriver(Settings.ServerUrl, options, TimeSpan.FromSeconds(90));
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Driver is not null)
        {
            try
            {
                if (_failureDirectory is not null)
                {
                    Directory.CreateDirectory(_failureDirectory);
                    File.WriteAllText(Path.Combine(_failureDirectory, "page-source.xml"), Driver.PageSource);
                }
            }
            finally
            {
                Driver.Quit();
                Driver.Dispose();
            }
        }

        return Task.CompletedTask;
    }

    public void SetFailureDirectory(string directory) => _failureDirectory = directory;

    public void ResetApplication()
    {
        Driver?.TerminateApp("com.companyname.gymtrackermobile");
        Driver?.ActivateApp("com.companyname.gymtrackermobile");
    }
}
