using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class StartWorkoutViewModelTests
{
    [Fact]
    public async Task Loading_exposes_all_templates_with_mockup_descriptions()
    {
        var templates = CreateTemplates();
        var viewModel = CreateViewModel(templates);

        await viewModel.LoadAsync();

        Assert.Equal(["Push", "Pull", "Legs", "Full Body"], viewModel.State.Templates.Select(x => x.Name));
        Assert.Equal("Chest · Shoulders · Triceps", viewModel.State.Templates[0].Description);
        Assert.Equal("Back · Biceps", viewModel.State.Templates[1].Description);
        Assert.Equal("Quads · Hamstrings · Glutes", viewModel.State.Templates[2].Description);
        Assert.Equal("A bit of everything", viewModel.State.Templates[3].Description);
    }

    [Fact]
    public async Task Templates_use_distinct_mockup_illustrations_and_colours()
    {
        var viewModel = CreateViewModel(CreateTemplates());

        await viewModel.LoadAsync();

        Assert.Equal(["neutral_workout_push.png", "neutral_workout_pull.png", "neutral_workout_legs.png", "neutral_workout_full_body.png"], viewModel.State.Templates.Select(x => x.IconSource));
        Assert.Equal(4, viewModel.State.Templates.Select(x => x.IconBackgroundColor).Distinct().Count());
    }

    [Fact]
    public async Task Selecting_illustration_style_updates_template_artwork()
    {
        var viewModel = CreateViewModel(CreateTemplates());
        await viewModel.LoadAsync();

        await viewModel.SelectIllustrationStyleAsync(IllustrationStyle.Male);

        Assert.Equal(IllustrationStyle.Male, viewModel.State.IllustrationStyle);
        Assert.Equal("male_workout_push.png", viewModel.State.Templates[0].IconSource);
    }

    [Fact]
    public async Task Selecting_template_exposes_confirmation_details_and_start_becomes_available()
    {
        var template = CreateTemplates()[0];
        var viewModel = CreateViewModel([template]);
        await viewModel.LoadAsync();

        viewModel.SelectTemplate(template.Id);

        Assert.Equal(template.Id, viewModel.State.SelectedTemplateId);
        Assert.Equal("Push", viewModel.State.SelectedTemplateName);
        Assert.True(viewModel.State.CanStart);
    }

    [Fact]
    public async Task Starting_selected_template_creates_workout_and_navigates_to_active_workout()
    {
        var template = CreateTemplates()[0];
        var repository = new RecordingWorkoutRepository { Result = new WorkoutSession { Id = Guid.NewGuid() } };
        var routes = new List<string>();
        var viewModel = CreateViewModel([template], repository, routes);
        await viewModel.LoadAsync();
        viewModel.SelectTemplate(template.Id);

        await viewModel.StartWorkoutAsync();

        Assert.Equal(template.Id, repository.StartedTemplateId);
        Assert.Equal(ActiveWorkoutRoutes.Page, routes.Single().Split('?')[0]);
        Assert.Contains(repository.Result.Id.ToString(), routes.Single());
    }

    [Fact]
    public async Task Loading_with_an_active_workout_exposes_resume_without_allowing_a_second_start()
    {
        var template = CreateTemplates()[0];
        var active = new WorkoutSession { Id = Guid.NewGuid(), TemplateName = template.Name, IsActive = true };
        var repository = new RecordingWorkoutRepository { Templates = [template], ActiveWorkout = active };
        var routes = new List<string>();
        var viewModel = CreateViewModel([template], repository, routes);

        await viewModel.LoadAsync();
        viewModel.SelectTemplate(template.Id);
        await viewModel.ResumeWorkoutAsync();
        await viewModel.StartWorkoutAsync();

        Assert.Equal(active.Id, viewModel.State.ActiveWorkoutId);
        Assert.Equal(ActiveWorkoutRoutes.For(active.Id), routes.Single());
        Assert.Equal(0, repository.StartCount);
        Assert.False(viewModel.State.CanStart);
    }

    [Fact]
    public async Task Duplicate_start_requests_are_ignored_while_the_first_request_is_in_flight()
    {
        var template = CreateTemplates()[0];
        var repository = new RecordingWorkoutRepository();
        var viewModel = CreateViewModel([template], repository);
        await viewModel.LoadAsync();
        viewModel.SelectTemplate(template.Id);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        repository.StartGate = release.Task;

        var first = viewModel.StartWorkoutAsync();
        var second = viewModel.StartWorkoutAsync();
        await Task.Delay(20);

        Assert.Equal(1, repository.StartCount);
        Assert.False(viewModel.State.CanStart);

        release.SetResult();
        await Task.WhenAll(first, second);
        Assert.True(viewModel.State.CanStart);
    }

    [Fact]
    public async Task Start_error_is_recoverable_and_keeps_selection()
    {
        var template = CreateTemplates()[0];
        var repository = new RecordingWorkoutRepository { Error = new InvalidOperationException("Storage unavailable") };
        var viewModel = CreateViewModel([template], repository);
        await viewModel.LoadAsync();
        viewModel.SelectTemplate(template.Id);

        await viewModel.StartWorkoutAsync();

        Assert.Equal("Storage unavailable", viewModel.State.ErrorMessage);
        Assert.Equal(template.Id, viewModel.State.SelectedTemplateId);
        Assert.True(viewModel.State.CanStart);
    }

    [Fact]
    public async Task Back_navigates_to_the_previous_screen()
    {
        var routes = new List<string>();
        var viewModel = CreateViewModel(CreateTemplates(), navigate: route =>
        {
            routes.Add(route);
            return Task.CompletedTask;
        });

        await viewModel.GoBackAsync();

        Assert.Equal("..", routes.Single());
    }

    private static StartWorkoutViewModel CreateViewModel(
        IReadOnlyList<WorkoutTemplate> templates,
        RecordingWorkoutRepository? repository = null,
        List<string>? routes = null,
        Func<string, Task>? navigate = null)
    {
        repository ??= new RecordingWorkoutRepository();
        repository.Templates = templates;
        return new(
            repository,
            new RecordingDatabaseInitializer(),
            navigate ?? (route =>
            {
                routes?.Add(route);
                return Task.CompletedTask;
            }));
    }

    private static WorkoutTemplate[] CreateTemplates() =>
    [
        new() { Id = Guid.NewGuid(), Name = "Push" },
        new() { Id = Guid.NewGuid(), Name = "Pull" },
        new() { Id = Guid.NewGuid(), Name = "Legs" },
        new() { Id = Guid.NewGuid(), Name = "Full Body" }
    ];

    private sealed class RecordingDatabaseInitializer : IDatabaseInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RecordingWorkoutRepository : IWorkoutRepository
    {
        public IReadOnlyList<WorkoutTemplate> Templates { get; set; } = [];
        public Guid StartedTemplateId { get; private set; }
        public int StartCount { get; private set; }
        public WorkoutSession Result { get; set; } = new();
        public WorkoutSession? ActiveWorkout { get; set; }
        public Exception? Error { get; set; }
        public Task? StartGate { get; set; }

        public Task<IReadOnlyList<WorkoutTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Templates);

        public async Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default)
        {
            StartCount++;
            StartedTemplateId = templateId;
            if (StartGate is not null) await StartGate;
            if (Error is not null) throw Error;
            return Result;
        }

        public Task<IReadOnlyList<WorkoutSession>> GetCompletedWorkoutsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default) => Task.FromResult(ActiveWorkout);
        public Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
