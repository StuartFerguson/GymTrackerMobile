namespace GymTrackerMobile.Domain;

public enum ExerciseMode { Machine, Barbell, Dumbbell, Bodyweight }

public enum WeightEntryConvention { TotalLoad, PerDumbbell, BodyweightOnly }

public enum SetStatus { Planned, Completed, Incomplete, Failed, Skipped }

public enum ActivityType { Walking, Running, Swimming }

public enum RecommendationStatus { Proposed, Accepted, Edited, Ignored }

public enum RecommendationOutcomeType { Accepted, Edited, Ignored }
