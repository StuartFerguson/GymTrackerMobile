namespace GymTrackerMobile.UI.Tests;

public sealed class UiAutomationIdentifierTests
{
    [Fact]
    public void Exposes_stable_identifiers_for_primary_journeys()
    {
        Assert.Equal("start-template-push", UiAutomationIds.StartTemplatePush);
        Assert.Equal("start-workout", UiAutomationIds.StartWorkout);
        Assert.Equal("active-weight-1", UiAutomationIds.ActiveWeight(1));
        Assert.Equal("active-repetitions-1", UiAutomationIds.ActiveRepetitions(1));
        Assert.Equal("active-save-set-1", UiAutomationIds.ActiveSaveSet(1));
        Assert.Equal("active-complete", UiAutomationIds.ActiveComplete);
        Assert.Equal("recommendation-accept", UiAutomationIds.RecommendationAccept);
        Assert.Equal("recommendation-edit", UiAutomationIds.RecommendationEdit);
        Assert.Equal("recommendation-ignore", UiAutomationIds.RecommendationIgnore);
        Assert.Equal("activity-type-walking", UiAutomationIds.ActivityType("Walking"));
        Assert.Equal("activity-save", UiAutomationIds.ActivitySave);
        Assert.Equal("history-item-0", UiAutomationIds.HistoryItem(0));
        Assert.Equal("history-progress-0", UiAutomationIds.HistoryProgress(0));
        Assert.Equal("workout-summary-name", UiAutomationIds.WorkoutSummaryName);
        Assert.Equal("workout-summary-completed-sets", UiAutomationIds.WorkoutSummaryCompletedSets);
        Assert.Equal("activity-summary-type", UiAutomationIds.ActivitySummaryType);
        Assert.Equal("progress-exercise-name", UiAutomationIds.ProgressExerciseName);
    }
}
