using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class DeveloperResetViewModelTests
{
    [Fact]
    public async Task Reset_does_not_call_service_when_confirmation_is_declined()
    {
        var service = new RecordingResetService();
        var viewModel = new DeveloperResetViewModel(service);

        var result = await viewModel.ResetAsync(() => Task.FromResult(false));

        Assert.False(result);
        Assert.False(service.WasCalled);
    }

    [Fact]
    public async Task Reset_calls_service_after_confirmation_and_reports_completion()
    {
        var service = new RecordingResetService();
        var viewModel = new DeveloperResetViewModel(service);

        var result = await viewModel.ResetAsync(() => Task.FromResult(true));

        Assert.True(result);
        Assert.True(service.WasCalled);
        Assert.Equal("Local app data was reset.", viewModel.StatusMessage);
        Assert.Null(viewModel.ErrorMessage);
    }

    private sealed class RecordingResetService : IAppDataResetService
    {
        public bool WasCalled { get; private set; }

        public Task ResetAsync(CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }
}
