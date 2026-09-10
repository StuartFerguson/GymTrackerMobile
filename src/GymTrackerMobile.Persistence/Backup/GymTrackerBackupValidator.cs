using System.Globalization;
using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Persistence.Backup;

public sealed class GymTrackerBackupValidator
{
    public BackupValidationResult Validate(string json)
    {
        var errors = new List<BackupValidationError>();
        GymTrackerBackupDocument? document;
        try { document = GymTrackerBackupJson.Deserialize(json); }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException)
        {
            var path = ex is System.Text.Json.JsonException jsonException ? jsonException.Path ?? "$" : "$";
            return new() { Errors = [new(path, $"Invalid JSON: {ex.Message}")] };
        }

        ValidateRequiredProperties(errors, json);
        if (document.Exercises is null || document.WorkoutTemplates is null || document.TemplateExercises is null ||
            document.WorkoutSessions is null || document.ActivityRecords is null || document.Recommendations is null ||
            document.UserSettings is null || document.BackupMetadata is null)
            return new() { Errors = errors };
        if (document.SchemaVersion != 1) Add(errors, "schemaVersion", "Unsupported schema version.");
        if (document.ExportedAtUtc.Kind != DateTimeKind.Utc) Add(errors, "exportedAtUtc", "Date must be UTC.");
        if (string.IsNullOrWhiteSpace(document.Application)) Add(errors, "application", "Application is required.");
        if (document.BackupMetadata is null) Add(errors, "backupMetadata", "Backup metadata is required.");

        ValidateIds(errors, "exercises", document.Exercises.Select(x => x.Id));
        ValidateIds(errors, "workoutTemplates", document.WorkoutTemplates.Select(x => x.Id));
        ValidateIds(errors, "templateExercises", document.TemplateExercises.Select(x => x.Id));
        ValidateIds(errors, "workoutSessions", document.WorkoutSessions.Select(x => x.Id));
        ValidateIds(errors, "activities", document.ActivityRecords.Select(x => x.Id));
        ValidateIds(errors, "recommendations", document.Recommendations.Select(x => x.Id));
        ValidateIds(errors, "settings", document.UserSettings.Select(x => x.Id));

        var exerciseIds = document.Exercises.Select(x => x.Id).ToHashSet();
        var templateIds = document.WorkoutTemplates.Select(x => x.Id).ToHashSet();
        var sessionIds = document.WorkoutSessions.Select(x => x.Id).ToHashSet();
        var workoutExerciseIds = document.WorkoutSessions.SelectMany(x => x.Exercises).Select(x => x.Id).ToHashSet();
        var names = document.WorkoutTemplates.GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1);
        foreach (var group in names) Add(errors, "workoutTemplates", $"Duplicate template name '{group.Key}'.");

        foreach (var (item, index) in document.Exercises.Select((x, i) => (x, i)))
        {
            var path = $"exercises[{index}]";
            if (string.IsNullOrWhiteSpace(item.Name)) Add(errors, path + ".name", "Name is required.");
            if (item.DefaultMinimumRepetitions < 1 || item.DefaultMaximumRepetitions < item.DefaultMinimumRepetitions)
                Add(errors, path + ".defaultMinimumRepetitions", "Repetition range is invalid.");
            if (item.DefaultSetCount < 1) Add(errors, path + ".defaultSetCount", "Set count must be positive.");
            ValidateEnum(errors, path + ".exerciseMode", item.ExerciseMode);
            ValidateEnum(errors, path + ".weightEntryConvention", item.WeightEntryConvention);
        }

        foreach (var (item, index) in document.TemplateExercises.Select((x, i) => (x, i)))
        {
            var path = $"templateExercises[{index}]";
            RequireReference(errors, path + ".workoutTemplateId", item.WorkoutTemplateId, templateIds);
            RequireReference(errors, path + ".exerciseId", item.ExerciseId, exerciseIds);
            if (item.SortOrder < 0) Add(errors, path + ".sortOrder", "Sort order cannot be negative.");
            if (item.TargetMinimumRepetitions < 1 || item.TargetMaximumRepetitions < item.TargetMinimumRepetitions)
                Add(errors, path + ".targetMinimumRepetitions", "Repetition range is invalid.");
        }

        foreach (var (session, sessionIndex) in document.WorkoutSessions.Select((x, i) => (x, i)))
        {
            var sessionPath = $"workoutSessions[{sessionIndex}]";
            RequireReference(errors, sessionPath + ".templateId", session.TemplateId, templateIds);
            ValidateUtc(errors, sessionPath + ".startedAtUtc", session.StartedAtUtc);
            if (session.CompletedAtUtc is { } completed) ValidateUtc(errors, sessionPath + ".completedAtUtc", completed);
            foreach (var (exercise, exerciseIndex) in session.Exercises.Select((x, i) => (x, i)))
            {
                var path = $"{sessionPath}.exercises[{exerciseIndex}]";
                RequireReference(errors, path + ".exerciseId", exercise.ExerciseId, exerciseIds);
                ValidateEnum(errors, path + ".weightEntryConvention", exercise.WeightEntryConvention);
                foreach (var (set, setIndex) in exercise.Sets.Select((x, i) => (x, i)))
                {
                    var setPath = $"{path}.sets[{setIndex}]";
                    if (set.SetNumber < 1) Add(errors, setPath + ".setNumber", "Set number must be positive.");
                    ValidateEnum(errors, setPath + ".status", set.Status);
                    if (set.WeightKilograms is < 0 or double.NaN or double.PositiveInfinity or double.NegativeInfinity)
                        Add(errors, setPath + ".weightKilograms", "Weight must be finite and non-negative.");
                    if (set.Repetitions is < 0) Add(errors, setPath + ".repetitions", "Repetitions cannot be negative.");
                    if (set.Rpe is < 0 or > 10) Add(errors, setPath + ".rpe", "RPE must be between 0 and 10.");
                    if (set.RecordedAtUtc is { } recorded) ValidateUtc(errors, setPath + ".recordedAtUtc", recorded);
                }
            }
        }

        foreach (var (activity, index) in document.ActivityRecords.Select((x, i) => (x, i)))
        {
            var path = $"activities[{index}]";
            ValidateUtc(errors, path + ".activityDateUtc", activity.ActivityDateUtc);
            ValidateEnum(errors, path + ".activityType", activity.ActivityType);
            if (activity.DurationMinutes is < 0) Add(errors, path + ".durationMinutes", "Duration cannot be negative.");
            if (activity.DistanceKilometres is < 0) Add(errors, path + ".distanceKilometres", "Distance cannot be negative.");
            if (activity.Steps is < 0) Add(errors, path + ".steps", "Steps cannot be negative.");
        }

        foreach (var (recommendation, index) in document.Recommendations.Select((x, i) => (x, i)))
        {
            var path = $"recommendations[{index}]";
            RequireReference(errors, path + ".workoutExerciseId", recommendation.WorkoutExerciseId, workoutExerciseIds);
            ValidateUtc(errors, path + ".createdAtUtc", recommendation.CreatedAtUtc);
            ValidateEnum(errors, path + ".status", recommendation.Status);
            if (recommendation.Confidence is < 0 or > 1) Add(errors, path + ".confidence", "Confidence must be between 0 and 1.");
            if (recommendation.Outcome is { } outcome) ValidateEnum(errors, path + ".outcome.outcomeType", outcome.OutcomeType);
        }

        foreach (var (setting, index) in document.UserSettings.Select((x, i) => (x, i)))
            if (string.IsNullOrWhiteSpace(setting.Key)) Add(errors, $"settings[{index}].key", "Setting key is required.");

        return new() { Document = errors.Count == 0 ? document : null, Errors = errors };
    }

    private static void ValidateIds(List<BackupValidationError> errors, string path, IEnumerable<Guid> ids)
    {
        var seen = new HashSet<Guid>();
        foreach (var (id, index) in ids.Select((id, i) => (id, i)))
        {
            if (id == Guid.Empty) Add(errors, $"{path}[{index}].id", "ID is required.");
            else if (!seen.Add(id)) Add(errors, $"{path}[{index}].id", "Duplicate ID.");
        }
    }

    private static void ValidateEnum<T>(List<BackupValidationError> errors, string path, T value) where T : struct, Enum
    { if (!Enum.IsDefined(value)) Add(errors, path, "Value is not supported."); }

    private static void ValidateUtc(List<BackupValidationError> errors, string path, DateTime value)
    { if (value.Kind != DateTimeKind.Utc) Add(errors, path, "Date must be UTC."); }

    private static void RequireReference(List<BackupValidationError> errors, string path, Guid id, HashSet<Guid> ids)
    { if (!ids.Contains(id)) Add(errors, path, "Referenced ID does not exist."); }

    private static void ValidateRequiredProperties(List<BackupValidationError> errors, string json)
    {
        using var parsed = System.Text.Json.JsonDocument.Parse(json);
        var root = parsed.RootElement;
        foreach (var name in new[] { "schemaVersion", "exportedAtUtc", "application", "exercises", "workoutTemplates", "templateExercises", "workoutSessions", "activityRecords", "recommendations", "userSettings", "backupMetadata" })
            if (!root.TryGetProperty(name, out var value) || value.ValueKind is System.Text.Json.JsonValueKind.Null or System.Text.Json.JsonValueKind.Undefined)
                Add(errors, name, "Field is required.");
    }

    private static void Add(List<BackupValidationError> errors, string path, string message) => errors.Add(new(path, message));
}
