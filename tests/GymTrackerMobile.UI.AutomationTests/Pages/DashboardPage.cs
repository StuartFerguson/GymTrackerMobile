using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public sealed class DashboardPage(AndroidDriver driver) : AppPage(driver)
{
    public StartWorkoutPage OpenStartWorkout()
    {
        Tap("dashboard-start-workout");
        return new StartWorkoutPage(Driver);
    }

    public ActivityPage OpenActivityLog()
    {
        Tap("dashboard-log-activity");
        return new ActivityPage(Driver);
    }

    public HistoryPage OpenHistory()
    {
        Tap("dashboard-history");
        return new HistoryPage(Driver);
    }
}
