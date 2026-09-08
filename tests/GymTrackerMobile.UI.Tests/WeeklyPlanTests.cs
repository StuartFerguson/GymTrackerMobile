using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class WeeklyPlanTests
{
    [Fact]
    public void Weekly_plan_has_a_dashboard_route()
    {
        Assert.Equal("weekly-plan", WeeklyPlanRoutes.Page);
    }

    [Fact]
    public void Builds_monday_to_sunday_with_templates_activities_and_rest()
    {
        var pushId = Guid.NewGuid();
        var pullId = Guid.NewGuid();
        var legsId = Guid.NewGuid();
        var fullBodyId = Guid.NewGuid();

        var days = WeeklyPlanStateBuilder.Build(
            [
                new WeeklyPlanTemplateSummary(pushId, "Push"),
                new WeeklyPlanTemplateSummary(pullId, "Pull"),
                new WeeklyPlanTemplateSummary(legsId, "Legs"),
                new WeeklyPlanTemplateSummary(fullBodyId, "Full Body")
            ]).Days;

        Assert.Equal(["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"], days.Select(x => x.DayName));
        Assert.Equal(["Push", "Pull", "Legs", "Full Body", "Walk", "Swim", "Rest"], days.Select(x => x.Title));
        Assert.Equal([WeeklyPlanDayKind.Gym, WeeklyPlanDayKind.Gym, WeeklyPlanDayKind.Gym, WeeklyPlanDayKind.Gym, WeeklyPlanDayKind.Activity, WeeklyPlanDayKind.Activity, WeeklyPlanDayKind.Rest], days.Select(x => x.Kind));
        Assert.Equal(pushId, days[0].TemplateId);
        Assert.Equal(fullBodyId, days[3].TemplateId);
    }

    [Fact]
    public void Actions_are_enabled_for_matching_gym_and_activity_days_only()
    {
        var state = WeeklyPlanStateBuilder.Build([new WeeklyPlanTemplateSummary(Guid.NewGuid(), "Push")]);

        Assert.True(state.Days[0].CanStartWorkout);
        Assert.False(state.Days[0].CanLogActivity);
        Assert.False(state.Days[1].CanStartWorkout);
        Assert.True(state.Days[4].CanLogActivity);
        Assert.False(state.Days[4].CanStartWorkout);
        Assert.False(state.Days[6].CanStartWorkout);
        Assert.False(state.Days[6].CanLogActivity);
    }

    [Fact]
    public void Days_expose_mockup_presentation_metadata()
    {
        var days = WeeklyPlanStateBuilder.Build([]).Days;

        Assert.Equal(["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"], days.Select(x => x.ShortDayName));
        Assert.Equal(["plan_dumbbell.svg", "plan_dumbbell.svg", "plan_dumbbell.svg", "plan_dumbbell.svg", "plan_walk.svg", "plan_swim.svg", "plan_rest.svg"], days.Select(x => x.IconSource));
        Assert.Equal(["Start workout", "Start workout", "Start workout", "Start workout", "Log activity", "Log activity", "Rest"], days.Select(x => x.ActionLabel));
    }

    [Fact]
    public async Task Day_actions_navigate_to_the_matching_flow()
    {
        var templateId = Guid.NewGuid();
        var routes = new List<string>();
        var viewModel = new WeeklyPlanViewModel(
            [new WeeklyPlanTemplateSummary(templateId, "Push")],
            route =>
            {
                routes.Add(route);
                return Task.CompletedTask;
            });

        await viewModel.StartWorkoutAsync(viewModel.State.Days[0]);
        await viewModel.LogActivityAsync(viewModel.State.Days[4]);

        Assert.Equal(
            [WeeklyPlanRoutes.StartWorkout(templateId), WeeklyPlanRoutes.LogActivity],
            routes);
    }
}
