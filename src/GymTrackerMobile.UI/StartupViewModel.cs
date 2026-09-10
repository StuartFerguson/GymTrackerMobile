using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class StartupViewModel(IDatabaseInitializer databaseInitializer)
{
    private readonly IDatabaseInitializer _databaseInitializer = databaseInitializer;
    private bool _isInitializing;

    public StartupState State { get; private set; } = new();

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitializing || State.IsReady)
        {
            return;
        }

        _isInitializing = true;
        State = new(IsLoading: true);

        try
        {
            await _databaseInitializer.InitializeAsync(cancellationToken);
            State = new(IsReady: true);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            State = new(ErrorMessage: exception.Message);
        }
        finally
        {
            _isInitializing = false;
        }
    }
}
