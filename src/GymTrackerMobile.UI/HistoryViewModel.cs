using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed record HistoryWorkoutItem(Guid Id, string Name, DateTime CompletedAtLocal, int CompletedSets, int PlannedSets);

public sealed class HistoryViewModel(IWorkoutRepository workouts)
{
    public IReadOnlyList<HistoryWorkoutItem> Items { get; private set; } = [];

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await workouts.GetCompletedWorkoutsAsync(DateTime.UtcNow.AddYears(-10), DateTime.UtcNow.AddDays(1), cancellationToken);
        Items = sessions.Where(x => x.CompletedAtUtc is not null).Select(x => new HistoryWorkoutItem(
            x.Id,
            x.TemplateName,
            x.CompletedAtUtc!.Value.ToLocalTime(),
            x.Exercises.SelectMany(y => y.Sets).Count(y => y.Status == GymTrackerMobile.Domain.SetStatus.Completed),
            x.Exercises.Sum(y => y.PlannedSetCount))).ToList();
    }
}
