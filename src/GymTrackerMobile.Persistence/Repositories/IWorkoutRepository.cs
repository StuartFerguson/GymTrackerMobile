using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Persistence;

public interface IWorkoutRepository
{
    Task<IReadOnlyList<WorkoutTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkoutSession>> GetCompletedWorkoutsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default);
    Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default);
    Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default);
    Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default);
}
