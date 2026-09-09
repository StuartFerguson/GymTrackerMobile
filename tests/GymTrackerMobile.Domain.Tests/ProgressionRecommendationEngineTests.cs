using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Domain.Tests;

public sealed class ProgressionRecommendationEngineTests
{
    [Fact]
    public void Comfortable_top_range_completion_increases_weight_and_resets_reps()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 10,
            sets: [Set(60, 10, "Comfortable"), Set(60, 10, "Comfortable")]);

        Assert.Equal(62.5, result.ProposedWeightKilograms);
        Assert.Equal(8, result.ProposedMinimumRepetitions);
        Assert.Equal(10, result.ProposedMaximumRepetitions);
        Assert.Contains("smallest", result.Explanation, StringComparison.OrdinalIgnoreCase);
        Assert.False(result.HasPainFlag);
    }

    [Fact]
    public void Below_top_completion_keeps_weight_and_adds_one_rep()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 12,
            sets: [Set(60, 10), Set(60, 10)]);

        Assert.Equal(60, result.ProposedWeightKilograms);
        Assert.Equal(11, result.ProposedMinimumRepetitions);
        Assert.Equal(12, result.ProposedMaximumRepetitions);
    }

    [Fact]
    public void Single_lower_bound_miss_keeps_weight()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 12,
            sets: [Set(60, 7), Set(60, 10)]);

        Assert.Equal(60, result.ProposedWeightKilograms);
        Assert.Contains("one", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Multiple_lower_bound_misses_reduce_weight()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 12,
            sets: [Set(60, 6), Set(60, 7)]);

        Assert.Equal(57.5, result.ProposedWeightKilograms);
        Assert.Contains("miss", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void High_difficulty_blocks_increase()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 10,
            sets: [Set(60, 10, "Hard"), Set(60, 10, "Hard")]);

        Assert.Equal(60, result.ProposedWeightKilograms);
        Assert.Contains("difficulty", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pain_blocks_increase_and_flags_exercise()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 10,
            sets: [Set(60, 10, "Pain reported"), Set(60, 10, "Comfortable")]);

        Assert.Equal(60, result.ProposedWeightKilograms);
        Assert.True(result.HasPainFlag);
        Assert.Contains("pain", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Two_declining_sessions_suggest_a_lighter_session()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 10,
            sets: [Set(60, 8)],
            history: [
                Session(Set(65, 8)),
                Session(Set(60, 7))
            ]);

        Assert.Equal(57.5, result.ProposedWeightKilograms);
        Assert.Contains("decline", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Missing_difficulty_uses_completion_as_fallback()
    {
        var result = Recommend(
            targetMin: 8,
            targetMax: 10,
            sets: [Set(60, 10, null), Set(60, 10, null)]);

        Assert.Equal(62.5, result.ProposedWeightKilograms);
    }

    private static ProgressionRecommendation Recommend(
        int targetMin,
        int targetMax,
        IReadOnlyList<ProgressionSet> sets,
        IReadOnlyList<IReadOnlyList<ProgressionSet>>? history = null) =>
        ProgressionRecommendationEngine.Recommend(new ProgressionInput(targetMin, targetMax, sets, history ?? []));

    private static ProgressionSet Set(double weight, int reps, string? difficulty = null) => new(weight, reps, SetStatus.Completed, difficulty);

    private static IReadOnlyList<ProgressionSet> Session(ProgressionSet set) => [set];
}
