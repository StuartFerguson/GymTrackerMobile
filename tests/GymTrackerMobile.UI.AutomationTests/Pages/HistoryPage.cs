using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public sealed class HistoryPage(AndroidDriver driver) : AppPage(driver)
{
    public SummaryPage OpenFirstWorkout()
    {
        Tap("history-item-0");
        return new SummaryPage(Driver);
    }

    public string ItemName(int index) => Find($"history-name-{index}").Text;
    public string ItemDetails(int index) => Find($"history-details-{index}").Text;
}
