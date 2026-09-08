namespace GymTrackerMobile.Domain;

public sealed class ActivityRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ActivityDateUtc { get; set; }
    public ActivityType ActivityType { get; set; }
    public int? DurationMinutes { get; set; }
    public double? DistanceKilometres { get; set; }
    public int? Steps { get; set; }
    public string? Notes { get; set; }
    public double? AveragePaceMinutesPerKilometre { get; set; }
}
