using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class GymTrackerDbContext(DbContextOptions<GymTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
    public DbSet<TemplateExercise> TemplateExercises => Set<TemplateExercise>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<WorkoutSet> WorkoutSets => Set<WorkoutSet>();
    public DbSet<ActivityRecord> ActivityRecords => Set<ActivityRecord>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<RecommendationOutcome> RecommendationOutcomes => Set<RecommendationOutcome>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    public DbSet<BackupMetadata> BackupMetadata => Set<BackupMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("Exercises");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PrimaryMuscleGroup).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EquipmentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.WeightEntryConvention).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.ExerciseMode).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<WorkoutTemplate>(entity =>
        {
            entity.ToTable("WorkoutTemplates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasMany(x => x.Exercises).WithOne(x => x.WorkoutTemplate)
                .HasForeignKey(x => x.WorkoutTemplateId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TemplateExercise>(entity =>
        {
            entity.ToTable("TemplateExercises");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Exercise).WithMany(x => x.TemplateExercises)
                .HasForeignKey(x => x.ExerciseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.WorkoutTemplateId, x.SortOrder }).IsUnique();
        });

        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.ToTable("WorkoutSessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TemplateName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.StartedAtUtc);
            entity.HasIndex(x => x.IsActive);
            entity.HasMany(x => x.Exercises).WithOne(x => x.WorkoutSession)
                .HasForeignKey(x => x.WorkoutSessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.ToTable("WorkoutExercises");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExerciseName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.PrimaryMuscleGroup).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EquipmentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.WeightEntryConvention).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => new { x.WorkoutSessionId, x.SortOrder }).IsUnique();
            entity.HasMany(x => x.Sets).WithOne(x => x.WorkoutExercise)
                .HasForeignKey(x => x.WorkoutExerciseId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkoutSet>(entity =>
        {
            entity.ToTable("WorkoutSets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.WorkoutExerciseId, x.SetNumber }).IsUnique();
        });

        modelBuilder.Entity<ActivityRecord>(entity =>
        {
            entity.ToTable("ActivityRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ActivityType).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.ActivityDateUtc, x.ActivityType });
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.ToTable("Recommendations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Explanation).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasOne<WorkoutExercise>().WithMany().HasForeignKey(x => x.WorkoutExerciseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Outcome).WithOne(x => x.Recommendation)
                .HasForeignKey<RecommendationOutcome>(x => x.RecommendationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecommendationOutcome>(entity =>
        {
            entity.ToTable("RecommendationOutcomes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OutcomeType).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.ToTable("UserSettings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(1000).IsRequired();
            entity.HasIndex(x => x.Key).IsUnique();
        });

        modelBuilder.Entity<BackupMetadata>(entity =>
        {
            entity.ToTable("BackupMetadata");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LastBackupFileName).HasMaxLength(260);
            entity.Property(x => x.LastBackupFileIdentity).HasMaxLength(200);
        });
    }
}
