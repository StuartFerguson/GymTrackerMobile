using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class ActivityLogViewModelTests
{
    [Theory]
    [InlineData(ActivityType.Walking)]
    [InlineData(ActivityType.Running)]
    [InlineData(ActivityType.Swimming)]
    public async Task Saves_an_activity_with_required_and_optional_fields(ActivityType type)
    {
        var repository = new RecordingActivityRepository();
        var viewModel = new ActivityLogViewModel(repository, new NoOpDatabaseInitializer());
        viewModel.SelectActivityType(type);
        viewModel.SetDate(new DateTime(2026, 9, 9));
        viewModel.DurationText = "45";
        viewModel.DistanceText = "5.25";
        viewModel.StepsText = "6200";
        viewModel.NotesText = "Easy pace";
        if (type == ActivityType.Swimming)
        {
            viewModel.PoolLengthText = "25";
            viewModel.PoolLengthsText = "210";
        }

        await viewModel.SaveAsync();

        Assert.True(viewModel.State.IsSaved);
        Assert.Null(viewModel.State.ErrorMessage);
        Assert.Equal(type, repository.Saved!.ActivityType);
        Assert.Equal(45, repository.Saved.DurationMinutes);
        Assert.Equal(5.25, repository.Saved.DistanceKilometres);
        Assert.Equal(6200, repository.Saved.Steps);
        Assert.Equal("Easy pace", repository.Saved.Notes);
    }

    [Fact]
    public async Task Invalid_optional_values_show_field_errors_and_preserve_valid_inputs()
    {
        var repository = new RecordingActivityRepository();
        var viewModel = new ActivityLogViewModel(repository, new NoOpDatabaseInitializer());
        viewModel.SelectActivityType(ActivityType.Walking);
        viewModel.SetDate(new DateTime(2026, 9, 9));
        viewModel.DurationText = "30";
        viewModel.DistanceText = "-2";
        viewModel.StepsText = "not-a-number";

        await viewModel.SaveAsync();

        Assert.False(viewModel.State.IsSaved);
        Assert.Null(repository.Saved);
        Assert.Null(viewModel.State.DurationError);
        Assert.Equal("Distance must be zero or greater.", viewModel.State.DistanceError);
        Assert.Equal("Steps must be a whole number of zero or greater.", viewModel.State.StepsError);
        Assert.Equal("30", viewModel.DurationText);
    }

    [Fact]
    public async Task Missing_required_fields_are_reported_without_attempting_to_save()
    {
        var repository = new RecordingActivityRepository();
        var viewModel = new ActivityLogViewModel(repository, new NoOpDatabaseInitializer());

        await viewModel.SaveAsync();

        Assert.False(viewModel.State.IsSaved);
        Assert.Equal("Choose an activity type.", viewModel.State.ActivityTypeError);
        Assert.Equal("Choose a date.", viewModel.State.DateError);
        Assert.Null(repository.Saved);
    }

    [Fact]
    public async Task Save_failure_exposes_error_and_keeps_the_form_values()
    {
        var viewModel = new ActivityLogViewModel(new FailingActivityRepository(), new NoOpDatabaseInitializer());
        viewModel.SelectActivityType(ActivityType.Running);
        viewModel.SetDate(new DateTime(2026, 9, 9));
        viewModel.DurationText = "20";

        await viewModel.SaveAsync();

        Assert.False(viewModel.State.IsSaved);
        Assert.Equal("Storage unavailable", viewModel.State.ErrorMessage);
        Assert.Equal("20", viewModel.DurationText);
    }

    [Fact]
    public async Task Swimming_uses_pool_length_and_lengths_to_calculate_distance()
    {
        var repository = new RecordingActivityRepository();
        var viewModel = new ActivityLogViewModel(repository, new NoOpDatabaseInitializer());
        viewModel.SelectActivityType(ActivityType.Swimming);
        viewModel.SetDate(new DateTime(2026, 9, 9));
        viewModel.PoolLengthText = "25";
        viewModel.PoolLengthsText = "40";

        await viewModel.SaveAsync();

        Assert.True(viewModel.State.IsSaved);
        Assert.Equal(25, repository.Saved!.PoolLengthMetres);
        Assert.Equal(40, repository.Saved.PoolLengths);
        Assert.Equal(1, repository.Saved.DistanceKilometres);
    }

    [Fact]
    public async Task Swimming_requires_pool_length_and_lengths()
    {
        var viewModel = new ActivityLogViewModel(new RecordingActivityRepository(), new NoOpDatabaseInitializer());
        viewModel.SelectActivityType(ActivityType.Swimming);
        viewModel.SetDate(new DateTime(2026, 9, 9));

        await viewModel.SaveAsync();

        Assert.Equal("Enter the pool length in metres.", viewModel.State.PoolLengthError);
        Assert.Equal("Enter the number of lengths.", viewModel.State.PoolLengthsError);
    }

    private sealed class NoOpDatabaseInitializer : IDatabaseInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RecordingActivityRepository : IActivityRepository
    {
        public ActivityRecord? Saved { get; private set; }

        public Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default)
        {
            Saved = activity;
            return Task.FromResult(activity);
        }

        public Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ActivityRecord>>([]);
    }

    private sealed class FailingActivityRepository : IActivityRepository
    {
        public Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default) =>
            Task.FromException<ActivityRecord>(new InvalidOperationException("Storage unavailable"));

        public Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ActivityRecord>>([]);
    }
}
