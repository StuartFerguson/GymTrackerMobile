using System.Globalization;
using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed record ActivitySummaryState(
    Guid? ActivityId = null,
    string ActivityType = "Activity",
    DateTime? ActivityDateUtc = null,
    string Details = "",
    string? Notes = null,
    string? ErrorMessage = null);

public sealed class ActivitySummaryViewModel(IActivityRepository activities)
{
    public ActivitySummaryState State { get; private set; } = new();

    public async Task LoadAsync(Guid activityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await activities.GetActivitiesAsync(DateTime.MinValue, DateTime.MaxValue, cancellationToken);
            var activity = records.FirstOrDefault(x => x.Id == activityId);
            if (activity is null)
            {
                State = new(ActivityId: activityId, ErrorMessage: "The activity details could not be found.");
                return;
            }

            State = new(activity.Id, activity.ActivityType.ToString(), activity.ActivityDateUtc, BuildDetails(activity), activity.Notes);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            State = new(ActivityId: activityId, ErrorMessage: exception.Message);
        }
    }

    private static string BuildDetails(ActivityRecord activity)
    {
        var details = new List<string>();
        if (activity.DurationMinutes is int duration) details.Add(FormatDuration(duration));
        if (activity.DistanceKilometres is double distance) details.Add($"{distance.ToString("0.##", CultureInfo.InvariantCulture)} km");
        if (activity.Steps is int steps) details.Add($"{steps.ToString("N0", CultureInfo.InvariantCulture)} steps");
        if (activity.PoolLengthMetres is int poolLength) details.Add($"{poolLength} m pool");
        if (activity.PoolLengths is int lengths) details.Add($"{lengths} lengths");
        return details.Count == 0 ? "Activity logged" : string.Join(" · ", details);
    }

    private static string FormatDuration(int durationMinutes) => $"{durationMinutes / 60}h {durationMinutes % 60}m";
}
