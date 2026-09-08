namespace GymTrackerMobile.Domain;

public sealed class Exercise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string PrimaryMuscleGroup { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public WeightEntryConvention WeightEntryConvention { get; set; }
    public int DefaultMinimumRepetitions { get; set; }
    public int DefaultMaximumRepetitions { get; set; }
    public int DefaultSetCount { get; set; }
    public ExerciseMode ExerciseMode { get; set; }
    public ICollection<TemplateExercise> TemplateExercises { get; set; } = new List<TemplateExercise>();
}

public sealed class WorkoutTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
    public ICollection<TemplateExercise> Exercises { get; set; } = new List<TemplateExercise>();
}

public sealed class TemplateExercise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkoutTemplateId { get; set; }
    public WorkoutTemplate? WorkoutTemplate { get; set; }
    public Guid ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }
    public int SortOrder { get; set; }
    public int TargetMinimumRepetitions { get; set; }
    public int TargetMaximumRepetitions { get; set; }
    public int PlannedSetCount { get; set; }
}

public sealed class WorkoutSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
}

public sealed class WorkoutExercise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkoutSessionId { get; set; }
    public WorkoutSession? WorkoutSession { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string PrimaryMuscleGroup { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public WeightEntryConvention WeightEntryConvention { get; set; }
    public int TargetMinimumRepetitions { get; set; }
    public int TargetMaximumRepetitions { get; set; }
    public int PlannedSetCount { get; set; }
    public int SortOrder { get; set; }
    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
}

public sealed class WorkoutSet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkoutExerciseId { get; set; }
    public WorkoutExercise? WorkoutExercise { get; set; }
    public int SetNumber { get; set; }
    public SetStatus Status { get; set; } = SetStatus.Planned;
    public double? WeightKilograms { get; set; }
    public int? Repetitions { get; set; }
    public int? Rpe { get; set; }
    public string? Difficulty { get; set; }
    public string? Notes { get; set; }
    public DateTime? RecordedAtUtc { get; set; }
}
