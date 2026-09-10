using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public sealed class ProgressPage(AndroidDriver driver) : AppPage(driver)
{
    public string ExerciseName() => Find("progress-exercise-name").Text;
}
