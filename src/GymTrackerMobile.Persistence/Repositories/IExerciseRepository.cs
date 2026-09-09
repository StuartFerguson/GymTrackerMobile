using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Persistence;

public interface IExerciseRepository
{
    Task<IReadOnlyList<Exercise>> GetExercisesAsync(CancellationToken cancellationToken = default);
}
