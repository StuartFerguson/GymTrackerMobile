using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public sealed class ActiveWorkoutPage(AndroidDriver driver) : AppPage(driver)
{
    public ActiveWorkoutPage EnterSet(int setNumber, string weight, string repetitions)
    {
        var weightInput = Find($"active-weight-{setNumber}");
        weightInput.Clear();
        weightInput.SendKeys(weight);
        var repetitionsInput = Find($"active-repetitions-{setNumber}");
        repetitionsInput.Clear();
        repetitionsInput.SendKeys(repetitions);
        Tap($"active-save-set-{setNumber}");
        return this;
    }

    public SummaryPage Complete()
    {
        Tap("active-complete");
        return new SummaryPage(Driver);
    }

    public ActiveWorkoutPage AcceptRecommendation()
    {
        Tap("recommendation-accept");
        return this;
    }

    public ActiveWorkoutPage EditRecommendation(string weight)
    {
        var input = Find("recommendation-edit-weight");
        input.Clear();
        input.SendKeys(weight);
        Tap("recommendation-edit");
        return this;
    }

    public ActiveWorkoutPage IgnoreRecommendation()
    {
        Tap("recommendation-ignore");
        return this;
    }

    public string RecommendationOutcome() => Find("recommendation-outcome").Text;
    public string Weight(int setNumber) => Find($"active-weight-{setNumber}").Text;
    public string ErrorMessage() => Find("active-error").Text;
}
