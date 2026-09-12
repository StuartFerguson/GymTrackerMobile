using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public abstract class AppPage(AndroidDriver driver)
{
    protected AndroidDriver Driver { get; } = driver;

    protected IWebElement Find(string automationId, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(60));
        Exception? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var element = Driver.FindElement(MobileBy.Id(automationId));
                if (element.Displayed) return element;
            }
            catch (Exception error) when (error is WebDriverException or InvalidOperationException)
            {
                lastError = error;
            }

            ScrollDown();
            Thread.Sleep(250);
        }

        throw new WebDriverTimeoutException($"Control '{automationId}' was not visible.", lastError);
    }

    protected void Tap(string automationId) => Find(automationId).Click();

    private void ScrollDown()
    {
        try
        {
            var size = Driver.Manage().Window.Size;
            ((IJavaScriptExecutor)Driver).ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
            {
                ["left"] = 0,
                ["top"] = Math.Min(200, size.Height / 4),
                ["width"] = size.Width,
                ["height"] = Math.Max(1, size.Height - 300),
                ["direction"] = "down",
                ["percent"] = 0.6
            });
        }
        catch (WebDriverException)
        {
            // Keep polling if the current driver cannot perform a scroll gesture.
        }
    }
}
