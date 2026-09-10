using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class StartupViewModelTests
{
    [Fact]
    public async Task Initialization_completes_in_ready_state()
    {
        var initializer = new RecordingInitializer();
        var viewModel = new StartupViewModel(initializer);

        await viewModel.InitializeAsync();

        Assert.True(viewModel.State.IsReady);
        Assert.False(viewModel.State.IsLoading);
        Assert.Null(viewModel.State.ErrorMessage);
        Assert.Equal(1, initializer.Attempts);
    }

    [Fact]
    public async Task Initialization_failure_exposes_retryable_error_state()
    {
        var initializer = new RecordingInitializer { Failure = new InvalidOperationException("Storage unavailable") };
        var viewModel = new StartupViewModel(initializer);

        await viewModel.InitializeAsync();

        Assert.False(viewModel.State.IsReady);
        Assert.False(viewModel.State.IsLoading);
        Assert.Equal("Storage unavailable", viewModel.State.ErrorMessage);
    }

    [Fact]
    public async Task Retry_after_failure_runs_initialization_again()
    {
        var initializer = new RecordingInitializer { Failure = new InvalidOperationException("Storage unavailable") };
        var viewModel = new StartupViewModel(initializer);

        await viewModel.InitializeAsync();
        initializer.Failure = null;
        await viewModel.InitializeAsync();

        Assert.True(viewModel.State.IsReady);
        Assert.Null(viewModel.State.ErrorMessage);
        Assert.Equal(2, initializer.Attempts);
    }

    private sealed class RecordingInitializer : IDatabaseInitializer
    {
        public Exception? Failure { get; set; }
        public int Attempts { get; private set; }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Attempts++;
            return Failure is null ? Task.CompletedTask : Task.FromException(Failure);
        }
    }
}
