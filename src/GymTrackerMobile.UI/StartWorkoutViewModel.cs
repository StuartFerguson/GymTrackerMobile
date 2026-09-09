using System.Windows.Input;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class StartWorkoutViewModel
{
    private readonly IWorkoutRepository _workouts;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly Func<string, Task> _navigate;
    private readonly IllustrationPreferenceViewModel _illustrationPreference;

    public StartWorkoutViewModel(
        IWorkoutRepository workouts,
        IDatabaseInitializer databaseInitializer,
        Func<string, Task>? navigate = null,
        IllustrationPreferenceViewModel? illustrationPreference = null)
    {
        _workouts = workouts;
        _databaseInitializer = databaseInitializer;
        _navigate = navigate ?? (route => Shell.Current.GoToAsync(route));
        _illustrationPreference = illustrationPreference ?? new IllustrationPreferenceViewModel(new InMemorySettingsRepository());
        StartCommand = new Command(async () => await StartWorkoutAsync());
        BackCommand = new Command(async () => await GoBackAsync());
    }

    public ICommand StartCommand { get; }
    public ICommand BackCommand { get; }
    public StartWorkoutState State { get; private set; } = new();

    public async Task LoadAsync(Guid? templateId = null, CancellationToken cancellationToken = default)
    {
        await _databaseInitializer.InitializeAsync(cancellationToken);
        await _illustrationPreference.LoadAsync(cancellationToken);
        var templates = (await _workouts.GetTemplatesAsync(cancellationToken))
            .OrderBy(x => x.Name switch { "Push" => 0, "Pull" => 1, "Legs" => 2, "Full Body" => 3, _ => 4 })
            .Select(template => StartWorkoutTemplatePresentation.Build(template, _illustrationPreference.SelectedStyle))
            .ToList();
        State = State with { Templates = templates, IllustrationStyle = _illustrationPreference.SelectedStyle };
        if (templateId is Guid id && templates.Any(x => x.Id == id)) SelectTemplate(id);
    }

    public async Task SelectIllustrationStyleAsync(IllustrationStyle style, CancellationToken cancellationToken = default)
    {
        await _illustrationPreference.SelectAsync(style, cancellationToken);
        State = State with
        {
            IllustrationStyle = _illustrationPreference.SelectedStyle,
            Templates = State.Templates.Select(template => template with
            {
                IconSource = IllustrationAssetResolver.Resolve(_illustrationPreference.SelectedStyle, template.Name switch
                {
                    "Push" => IllustrationAssetKey.Push,
                    "Pull" => IllustrationAssetKey.Pull,
                    "Legs" => IllustrationAssetKey.Legs,
                    _ => IllustrationAssetKey.FullBody
                })
            }).ToList(),
            ErrorMessage = _illustrationPreference.ErrorMessage
        };
    }

    public void SelectTemplate(Guid templateId)
    {
        var template = State.Templates.FirstOrDefault(x => x.Id == templateId);
        if (template is null) return;
        State = State with { SelectedTemplateId = template.Id, SelectedTemplateName = template.Name, ErrorMessage = null };
    }

    public async Task StartWorkoutAsync(CancellationToken cancellationToken = default)
    {
        if (State.SelectedTemplateId is not Guid templateId || State.IsStarting) return;
        State = State with { IsStarting = true, ErrorMessage = null };
        try
        {
            var session = await _workouts.StartWorkoutAsync(templateId, DateTime.UtcNow, cancellationToken);
            await _navigate(ActiveWorkoutRoutes.For(session.Id));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            State = State with { IsStarting = false, ErrorMessage = exception.Message };
            return;
        }

        State = State with { IsStarting = false };
    }

    public Task GoBackAsync() => _navigate("..");

    private sealed class InMemorySettingsRepository : ISettingsRepository
    {
        public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
        public Task SetSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
