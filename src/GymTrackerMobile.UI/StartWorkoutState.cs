using GymTrackerMobile.Domain;

namespace GymTrackerMobile.UI;

public sealed record StartWorkoutTemplate(
    Guid Id,
    string Name,
    string Description,
    string IconSource,
    Color AccentColor,
    Color IconBackgroundColor,
    int ExerciseCount);

public sealed record StartWorkoutState
{
    public IReadOnlyList<StartWorkoutTemplate> Templates { get; init; } = [];
    public IllustrationStyle IllustrationStyle { get; init; } = IllustrationStyle.Neutral;
    public Guid? SelectedTemplateId { get; init; }
    public string SelectedTemplateName { get; init; } = string.Empty;
    public bool IsStarting { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? ActiveWorkoutId { get; init; }
    public string ActiveWorkoutName { get; init; } = string.Empty;
    public bool HasActiveWorkout => ActiveWorkoutId is not null;
    public bool CanResume => HasActiveWorkout && !IsStarting;
    public bool CanStart => SelectedTemplateId is not null && !IsStarting && !HasActiveWorkout;
}

public static class StartWorkoutTemplatePresentation
{
    public static StartWorkoutTemplate Build(WorkoutTemplate template, IllustrationStyle style = IllustrationStyle.Neutral) =>
        new(
            template.Id,
            template.Name,
            template.Name switch
            {
                "Push" => "Chest · Shoulders · Triceps",
                "Pull" => "Back · Biceps",
                "Legs" => "Quads · Hamstrings · Glutes",
                "Full Body" => "A bit of everything",
                _ => "A complete workout",
            },
            IllustrationAssetResolver.Resolve(style, template.Name switch
            {
                "Push" => IllustrationAssetKey.Push,
                "Pull" => IllustrationAssetKey.Pull,
                "Legs" => IllustrationAssetKey.Legs,
                _ => IllustrationAssetKey.FullBody,
            }),
            template.Name switch
            {
                "Push" => Color.FromArgb("#F47B20"),
                "Pull" => Color.FromArgb("#169F9A"),
                "Legs" => Color.FromArgb("#008A70"),
                _ => Color.FromArgb("#6A50B5"),
            },
            template.Name switch
            {
                "Push" => Color.FromArgb("#FFF0E4"),
                "Pull" => Color.FromArgb("#E3F7F8"),
                "Legs" => Color.FromArgb("#DDF6ED"),
                _ => Color.FromArgb("#F0EAFE"),
            },
            template.Exercises.Count);
}
