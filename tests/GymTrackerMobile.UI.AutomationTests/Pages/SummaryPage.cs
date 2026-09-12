using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public sealed class SummaryPage(AndroidDriver driver) : AppPage(driver)
{
    public string WorkoutName() => Find("workout-summary-name").Text;
    public string CompletedSets() => Find("workout-summary-completed-sets").Text;
    public string ActivityType() => Find("activity-summary-type").Text;

    public ProgressPage OpenProgress() { Tap("history-progress-0"); return new ProgressPage(Driver); }
}
