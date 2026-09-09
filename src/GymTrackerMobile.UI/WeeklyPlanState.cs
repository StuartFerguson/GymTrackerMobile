namespace GymTrackerMobile.UI;

public enum WeeklyPlanDayKind
{
    Gym,
    Activity,
    Rest
}

public sealed record WeeklyPlanTemplateSummary(Guid Id, string Name);

public sealed record WeeklyPlanDay(
    string DayName,
    int DayNumber,
    WeeklyPlanDayKind Kind,
    string Title,
    string Detail,
    Guid? TemplateId,
    bool CanStartWorkout,
    bool CanLogActivity,
    string ShortDayName,
    string IconSource,
    string ActionLabel);

public sealed class WeeklyPlanState
{
    public IReadOnlyList<WeeklyPlanDay> Days { get; init; } = [];
}

public static class WeeklyPlanStateBuilder
{
    private static readonly (DayOfWeek Day, string Title, WeeklyPlanDayKind Kind, string Detail)[] Plan =
    [
        (DayOfWeek.Monday, "Push", WeeklyPlanDayKind.Gym, "Chest · Shoulders · Triceps"),
        (DayOfWeek.Tuesday, "Walk", WeeklyPlanDayKind.Activity, "30–60 min · Keep it easy"),
        (DayOfWeek.Wednesday, "Pull", WeeklyPlanDayKind.Gym, "Back · Biceps"),
        (DayOfWeek.Thursday, "Swim", WeeklyPlanDayKind.Activity, "20–45 min · Steady pace"),
        (DayOfWeek.Friday, "Legs", WeeklyPlanDayKind.Gym, "Quads · Hamstrings · Glutes"),
        (DayOfWeek.Saturday, "Full Body", WeeklyPlanDayKind.Gym, "Compound lifts · Core"),
        (DayOfWeek.Sunday, "Rest", WeeklyPlanDayKind.Rest, "Recover · Be ready for next week")
    ];

    public static WeeklyPlanState Build(IReadOnlyList<WeeklyPlanTemplateSummary> templates, IllustrationStyle style = IllustrationStyle.Neutral)
    {
        var days = Plan.Select((plan, index) =>
        {
            var template = templates.FirstOrDefault(x => string.Equals(x.Name, plan.Title, StringComparison.OrdinalIgnoreCase));
            return new WeeklyPlanDay(
                plan.Day.ToString(),
                index + 1,
                plan.Kind,
                plan.Title,
                plan.Detail,
                template?.Id,
                plan.Kind == WeeklyPlanDayKind.Gym && template is not null,
                plan.Kind == WeeklyPlanDayKind.Activity,
                plan.Day.ToString()[..3],
                IllustrationAssetResolver.Resolve(style, plan.Kind switch
                {
                    WeeklyPlanDayKind.Gym => plan.Title switch
                    {
                        "Push" => IllustrationAssetKey.Push,
                        "Pull" => IllustrationAssetKey.Pull,
                        "Legs" => IllustrationAssetKey.Legs,
                        _ => IllustrationAssetKey.FullBody
                    },
                    WeeklyPlanDayKind.Activity when plan.Title == "Walk" => IllustrationAssetKey.Walk,
                    WeeklyPlanDayKind.Activity => IllustrationAssetKey.Swim,
                    _ => IllustrationAssetKey.Rest
                }),
                plan.Kind switch
                {
                    WeeklyPlanDayKind.Gym => "Start workout",
                    WeeklyPlanDayKind.Activity => "Log activity",
                    _ => "Rest"
                });
        }).ToList();

        return new WeeklyPlanState { Days = days };
    }
}
