namespace GymTrackerMobile.UI;

public static class UiAutomationIds
{
    public const string StartWorkout = "start-workout";
    public const string ActiveComplete = "active-complete";
    public const string RecommendationAccept = "recommendation-accept";
    public const string RecommendationEdit = "recommendation-edit";
    public const string RecommendationIgnore = "recommendation-ignore";
    public const string ActivitySave = "activity-save";
    public const string WorkoutSummaryName = "workout-summary-name";
    public const string WorkoutSummaryCompletedSets = "workout-summary-completed-sets";
    public const string ActivitySummaryType = "activity-summary-type";
    public const string ProgressExerciseName = "progress-exercise-name";

    public const string StartTemplatePush = "start-template-push";

    public static string ActiveWeight(int setNumber) => $"active-weight-{setNumber}";
    public static string ActiveRepetitions(int setNumber) => $"active-repetitions-{setNumber}";
    public static string ActiveSaveSet(int setNumber) => $"active-save-set-{setNumber}";
    public static string ActivityType(string type) => $"activity-type-{type.ToLowerInvariant()}";
    public static string HistoryItem(int index) => $"history-item-{index}";
    public static string HistoryProgress(int index) => $"history-progress-{index}";
}
