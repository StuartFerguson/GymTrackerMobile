using System.Globalization;
using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class ActivityLogViewModel(IActivityRepository activities, IDatabaseInitializer databaseInitializer)
{
    public ActivityLogState State { get; private set; } = new();
    public string DurationText { get; set; } = string.Empty;
    public string DistanceText { get; set; } = string.Empty;
    public string StepsText { get; set; } = string.Empty;
    public string PoolLengthText { get; set; } = string.Empty;
    public string PoolLengthsText { get; set; } = string.Empty;
    public string NotesText { get; set; } = string.Empty;

    public void SelectActivityType(ActivityType type) => State = State with { ActivityType = type, ActivityTypeError = null, ErrorMessage = null, IsSaved = false };

    public void SetDate(DateTime date) => State = State with { Date = date.Date, DateError = null, ErrorMessage = null, IsSaved = false };

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        State = State with { IsSaving = true, IsSaved = false, ErrorMessage = null };
        var errors = Validate();
        if (errors is not null)
        {
            State = errors with { IsSaving = false };
            return;
        }

        try
        {
            await databaseInitializer.InitializeAsync(cancellationToken);
            await activities.SaveActivityAsync(new ActivityRecord
            {
                ActivityDateUtc = State.Date!.Value,
                ActivityType = State.ActivityType!.Value,
                DurationMinutes = ParseInt(DurationText),
                DistanceKilometres = State.ActivityType == ActivityType.Swimming
                    ? CalculateSwimmingDistance()
                    : ParseDouble(DistanceText),
                Steps = ParseInt(StepsText),
                PoolLengthMetres = State.ActivityType == ActivityType.Swimming ? ParseInt(PoolLengthText) : null,
                PoolLengths = State.ActivityType == ActivityType.Swimming ? ParseInt(PoolLengthsText) : null,
                Notes = string.IsNullOrWhiteSpace(NotesText) ? null : NotesText.Trim()
            }, cancellationToken);
            State = State with { IsSaving = false, IsSaved = true };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            State = State with { IsSaving = false, ErrorMessage = exception.Message };
        }
    }

    private ActivityLogState? Validate()
    {
        var next = State with
        {
            ActivityTypeError = State.ActivityType is null ? "Choose an activity type." : null,
            DateError = State.Date is null ? "Choose a date." : null,
            DurationError = ValidateInt(DurationText, "Duration must be a whole number of minutes greater than zero.", requirePositive: true),
            DistanceError = State.ActivityType == ActivityType.Swimming ? null : ValidateDouble(DistanceText, "Distance must be zero or greater."),
            StepsError = ValidateInt(StepsText, "Steps must be a whole number of zero or greater.", requirePositive: false),
            PoolLengthError = State.ActivityType == ActivityType.Swimming ? ValidateOptionalPositiveInt(PoolLengthText, "Pool length must be a whole number greater than zero.") : null,
            PoolLengthsError = State.ActivityType == ActivityType.Swimming ? ValidateOptionalPositiveInt(PoolLengthsText, "Lengths must be a whole number greater than zero.") : null
        };
        return next.ActivityTypeError is null && next.DateError is null && next.DurationError is null && next.DistanceError is null && next.StepsError is null && next.PoolLengthError is null && next.PoolLengthsError is null ? null : next;
    }

    private static string? ValidateInt(string value, string message, bool requirePositive)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || (requirePositive ? result <= 0 : result < 0) ? message : null;
    }

    private static string? ValidateDouble(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) || result < 0 ? message : null;
    }

    private static string? ValidateOptionalPositiveInt(string value, string message) =>
        string.IsNullOrWhiteSpace(value) ? null : !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || result <= 0 ? message : null;

    private static int? ParseInt(string value) => string.IsNullOrWhiteSpace(value) ? null : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
    private static double? ParseDouble(string value) => string.IsNullOrWhiteSpace(value) ? null : double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

    private double? CalculateSwimmingDistance() =>
        ParseInt(PoolLengthText) is int poolLength && ParseInt(PoolLengthsText) is int lengths
            ? poolLength * lengths / 1000d
            : null;
}
