using GymTrackerMobile.Domain;

namespace GymTrackerMobile.UI;

public sealed record DashboardTemplateSummary(Guid Id, string Name);

public sealed record DashboardWorkoutSummary(string Name, DateTime CompletedAtUtc);

public sealed record DashboardActivitySummary(ActivityType Type, DateTime ActivityDateUtc, int? DurationMinutes);

public sealed record DashboardRecentItem(string Title, string Detail, DateTime DateUtc);

public sealed class DashboardState
{
    public string NextSessionName { get; init; } = string.Empty;
    public string NextSessionDay { get; init; } = string.Empty;
    public Guid? NextTemplateId { get; init; }
    public bool ShowGymQuickStart { get; init; }
    public bool ShowActivityQuickStart { get; init; }
    public bool IsEmptyState { get; init; }
    public string TrainingSummary { get; init; } = string.Empty;
    public IReadOnlyList<DashboardRecentItem> RecentItems { get; init; } = [];
}

public static class DashboardStateBuilder
{
    public static DashboardState Build(
        DateTime date,
        IReadOnlyList<DashboardTemplateSummary> templates,
        IReadOnlyList<DashboardWorkoutSummary> workouts,
        IReadOnlyList<DashboardActivitySummary> activities)
    {
        var today = date.Date;
        var planned = FindNextPlan(today, templates);
        var recentItems = workouts
            .Select(x => new DashboardRecentItem(x.Name, "Workout", x.CompletedAtUtc))
            .Concat(activities.Select(x => new DashboardRecentItem(
                x.Type.ToString(),
                x.DurationMinutes is null ? "Activity" : $"Activity · {FormatDuration(x.DurationMinutes.Value)}",
                x.ActivityDateUtc)))
            .OrderByDescending(x => x.DateUtc)
            .Take(5)
            .ToList();

        var dayPlan = WeeklyPlan.For(today.DayOfWeek);
        var isToday = planned.Date == today;
        var hasHistory = workouts.Count > 0 || activities.Count > 0;

        return new DashboardState
        {
            NextSessionName = planned.Name,
            NextSessionDay = planned.Date.ToString("dddd"),
            NextTemplateId = planned.TemplateId,
            ShowGymQuickStart = isToday && dayPlan.Kind == DashboardDayKind.Gym && planned.TemplateId is not null,
            ShowActivityQuickStart = isToday && dayPlan.Kind == DashboardDayKind.Activity,
            IsEmptyState = !hasHistory,
            TrainingSummary = $"{workouts.Count} {Pluralize(workouts.Count, "workout", "workouts")} · {activities.Count} {Pluralize(activities.Count, "activity", "activities")}",
            RecentItems = recentItems
        };
    }

    private static PlannedDay FindNextPlan(DateTime today, IReadOnlyList<DashboardTemplateSummary> templates)
    {
        for (var offset = 0; offset < 7; offset++)
        {
            var date = today.AddDays(offset);
            var plan = WeeklyPlan.For(date.DayOfWeek);
            if (plan.Kind == DashboardDayKind.Rest) continue;
            var template = templates.FirstOrDefault(x => string.Equals(x.Name, plan.Name, StringComparison.OrdinalIgnoreCase));
            if (plan.Kind == DashboardDayKind.Activity || template is not null)
            {
                return new PlannedDay(date, plan.Name, template?.Id);
            }
        }

        return new PlannedDay(today, "Rest", null);
    }

    private static string Pluralize(int count, string singular, string plural) => count == 1 ? singular : plural;

    private static string FormatDuration(int durationMinutes) => $"{durationMinutes / 60}h {durationMinutes % 60}m";

    private sealed record PlannedDay(DateTime Date, string Name, Guid? TemplateId);
}

public enum DashboardDayKind
{
    Gym,
    Activity,
    Rest
}

public sealed record DashboardDayPlan(string Name, DashboardDayKind Kind);

public static class WeeklyPlan
{
    public static DashboardDayPlan For(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Monday => new("Push", DashboardDayKind.Gym),
        DayOfWeek.Tuesday => new("Pull", DashboardDayKind.Gym),
        DayOfWeek.Wednesday => new("Activity", DashboardDayKind.Activity),
        DayOfWeek.Thursday => new("Legs", DashboardDayKind.Gym),
        DayOfWeek.Friday => new("Rest", DashboardDayKind.Rest),
        DayOfWeek.Saturday => new("Full Body", DashboardDayKind.Gym),
        _ => new("Rest", DashboardDayKind.Rest)
    };
}
