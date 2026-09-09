using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class ExerciseRepository(GymTrackerDbContext context) : IExerciseRepository
{
    public async Task<IReadOnlyList<Exercise>> GetExercisesAsync(CancellationToken cancellationToken = default) =>
        await context.Exercises.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
}
