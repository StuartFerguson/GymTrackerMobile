namespace GymTrackerMobile.Domain;

public sealed class Recommendation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkoutExerciseId { get; set; }
    public double? ProposedWeightKilograms { get; set; }
    public int? ProposedMinimumRepetitions { get; set; }
    public int? ProposedMaximumRepetitions { get; set; }
    public int? ProposedSetCount { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public RecommendationStatus Status { get; set; } = RecommendationStatus.Proposed;
    public DateTime CreatedAtUtc { get; set; }
    public RecommendationOutcome? Outcome { get; set; }
}

public sealed class RecommendationOutcome
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecommendationId { get; set; }
    public Recommendation? Recommendation { get; set; }
    public RecommendationOutcomeType OutcomeType { get; set; }
    public double? AppliedWeightKilograms { get; set; }
    public int? AppliedMinimumRepetitions { get; set; }
    public int? AppliedMaximumRepetitions { get; set; }
    public int? AppliedSetCount { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}
