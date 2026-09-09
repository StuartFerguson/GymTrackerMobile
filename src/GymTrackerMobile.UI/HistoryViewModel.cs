using System.Globalization;
using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public enum HistoryItemKind { Workout, Activity }

public sealed record HistoryItem(
    HistoryItemKind Kind,
    Guid Id,
    string Name,
    DateTime OccurredAtLocal,
    string Details,
    string? Notes,
    int CompletedSets = 0,
    int PlannedSets = 0)
{
    public bool IsWorkout => Kind == HistoryItemKind.Workout;
}

public sealed class HistoryViewModel(IWorkoutRepository workouts, IActivityRepository activities)
{
    public IReadOnlyList<HistoryItem> Items { get; private set; } = [];

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var fromUtc = DateTime.UtcNow.AddYears(-10);
        var toUtc = DateTime.UtcNow.AddDays(1);
        var sessions = await workouts.GetCompletedWorkoutsAsync(fromUtc, toUtc, cancellationToken);
        var activityRecords = await activities.GetActivitiesAsync(fromUtc, toUtc, cancellationToken);

        var workoutItems = sessions.Where(x => !x.IsActive && x.CompletedAtUtc is not null).Select(x =>
        {
            var completedSets = x.Exercises.SelectMany(y => y.Sets).Count(y => y.Status == SetStatus.Completed);
            var plannedSets = x.Exercises.Sum(y => y.PlannedSetCount);
            return new HistoryItem(HistoryItemKind.Workout, x.Id, x.TemplateName, x.CompletedAtUtc!.Value.ToLocalTime(), $"{completedSets} / {plannedSets} sets", x.Notes, completedSets, plannedSets);
        });
        var activityItems = activityRecords.Select(x => new HistoryItem(
            HistoryItemKind.Activity,
            x.Id,
            x.ActivityType.ToString(),
            x.ActivityDateUtc.ToLocalTime(),
            BuildActivityDetails(x),
            x.Notes));

        Items = workoutItems.Concat(activityItems).OrderByDescending(x => x.OccurredAtLocal).ToList();
    }

    private static string BuildActivityDetails(ActivityRecord activity)
    {
        var details = new List<string>();
        if (activity.DurationMinutes is int duration) details.Add($"{duration} min");
        if (activity.DistanceKilometres is double distance) details.Add($"{distance.ToString("0.##", CultureInfo.InvariantCulture)} km");
        if (activity.Steps is int steps) details.Add($"{steps:N0} steps");
        return details.Count == 0 ? "Activity logged" : string.Join(" · ", details);
    }
}
