namespace GymTrackerMobile.UI;

public sealed record StartupState(
    bool IsLoading = false,
    bool IsReady = false,
    string? ErrorMessage = null);
