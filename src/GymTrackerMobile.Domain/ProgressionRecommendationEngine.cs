namespace GymTrackerMobile.Domain;

public sealed record ProgressionSet(
    double? WeightKilograms,
    int? Repetitions,
    SetStatus Status,
    string? Difficulty = null);

public sealed record ProgressionInput(
    int TargetMinimumRepetitions,
    int TargetMaximumRepetitions,
    IReadOnlyList<ProgressionSet> Sets,
    IReadOnlyList<IReadOnlyList<ProgressionSet>> RecentSessions);

public sealed record ProgressionRecommendation(
    double? ProposedWeightKilograms,
    int ProposedMinimumRepetitions,
    int ProposedMaximumRepetitions,
    string Explanation,
    double Confidence,
    bool HasPainFlag);

public static class ProgressionRecommendationEngine
{
    public const double SmallestWeightIncrementKilograms = 2.5;

    public static ProgressionRecommendation Recommend(ProgressionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.TargetMinimumRepetitions <= 0 || input.TargetMaximumRepetitions < input.TargetMinimumRepetitions)
            throw new ArgumentException("The target repetition range is invalid.", nameof(input));

        var completed = input.Sets.Where(x => x.Status == SetStatus.Completed && x.Repetitions is > 0).ToList();
        var currentWeight = completed.Select(x => x.WeightKilograms).FirstOrDefault(x => x.HasValue);
        var pain = completed.Any(x => ContainsAny(x.Difficulty, "pain", "injur", "discomfort"));
        var highDifficulty = completed.Any(x => ContainsAny(x.Difficulty, "hard", "high", "very difficult", "max effort"));

        if (completed.Count == 0)
            return Result(currentWeight, input, "No completed sets yet, so keep the current target until there is enough evidence.", 0.25, pain);

        if (HasDecliningSessions(input.RecentSessions))
            return Result(DecreaseWeight(currentWeight), input, "Recent sessions show a decline, so a lighter session is suggested.", 0.8, pain);

        var misses = completed.Count(x => x.Repetitions < input.TargetMinimumRepetitions);
        if (misses >= 2)
            return Result(DecreaseWeight(currentWeight), input, "Multiple lower-bound misses suggest reducing the weight before progressing.", 0.85, pain);

        if (pain)
            return Result(currentWeight, input, "Pain was reported, so automatic increases are blocked and the exercise is flagged.", 0.95, true);

        if (highDifficulty)
            return Result(currentWeight, input, "Difficulty was high, so the weight is held rather than increased.", 0.85, false);

        if (misses == 1)
            return Result(currentWeight, input, "One lower-bound miss is not enough to change the weight, so hold it for another session.", 0.7, false);

        if (completed.All(x => x.Repetitions >= input.TargetMaximumRepetitions))
            return Result(IncreaseWeight(currentWeight), input, "All completed sets reached the top of the range; increase by the smallest increment and reset reps to the lower range.", 0.9, false);

        var nextMinimum = Math.Min(input.TargetMaximumRepetitions, completed.Max(x => x.Repetitions!.Value) + 1);
        return new(currentWeight, nextMinimum, input.TargetMaximumRepetitions, "Completion was below the top of the range, so keep the weight and add one rep to the target.", 0.75, false);
    }

    private static ProgressionRecommendation Result(double? weight, ProgressionInput input, string explanation, double confidence, bool pain) =>
        new(weight, input.TargetMinimumRepetitions, input.TargetMaximumRepetitions, explanation, confidence, pain);

    private static double? IncreaseWeight(double? weight) => weight is double value ? value + SmallestWeightIncrementKilograms : null;

    private static double? DecreaseWeight(double? weight) => weight is double value ? Math.Max(0, value - SmallestWeightIncrementKilograms) : null;

    private static bool HasDecliningSessions(IReadOnlyList<IReadOnlyList<ProgressionSet>> sessions)
    {
        if (sessions.Count < 2) return false;
        var previous = AverageRepetitions(sessions[^2]);
        var latest = AverageRepetitions(sessions[^1]);
        return previous is double previousValue && latest is double latestValue && latestValue < previousValue;
    }

    private static double? AverageRepetitions(IReadOnlyList<ProgressionSet> session)
    {
        var reps = session.Where(x => x.Status == SetStatus.Completed).Select(x => x.Repetitions).OfType<int>().ToList();
        return reps.Count == 0 ? null : reps.Average();
    }

    private static bool ContainsAny(string? value, params string[] terms) =>
        value is not null && terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
}
