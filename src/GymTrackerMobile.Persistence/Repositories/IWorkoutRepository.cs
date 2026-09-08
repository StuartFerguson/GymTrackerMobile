using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Persistence;

public interface IWorkoutRepository
{
    Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default);
    Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default);
    Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default);
    Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default);
}
