using GymTrackerMobile.Domain;

namespace GymTrackerMobile.UI;

public sealed record ActivityLogState
{
    public ActivityType? ActivityType { get; init; }
    public DateTime? Date { get; init; }
    public bool IsSaving { get; init; }
    public bool IsSaved { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ActivityTypeError { get; init; }
    public string? DateError { get; init; }
    public string? DurationError { get; init; }
    public string? DistanceError { get; init; }
    public string? StepsError { get; init; }
    public string? PoolLengthError { get; init; }
    public string? PoolLengthsError { get; init; }
}
