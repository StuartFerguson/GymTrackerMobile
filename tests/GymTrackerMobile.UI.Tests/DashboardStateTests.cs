using GymTrackerMobile.Domain;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class DashboardStateTests
{
    [Fact]
    public void Quick_start_routes_point_to_the_matching_flow()
    {
        Assert.Equal("start-workout", DashboardRoutes.StartWorkout);
        Assert.Equal("log-activity", DashboardRoutes.LogActivity);
    }

    [Fact]
    public void Monday_shows_the_push_gym_session_and_gym_quick_start()
    {
        var state = DashboardStateBuilder.Build(
            new DateTime(2026, 9, 7),
            [new DashboardTemplateSummary(Guid.NewGuid(), "Push")],
            [],
            []);

        Assert.Equal("Push", state.NextSessionName);
        Assert.Equal("Monday", state.NextSessionDay);
        Assert.True(state.ShowGymQuickStart);
        Assert.False(state.ShowActivityQuickStart);
    }

    [Fact]
    public void Wednesday_shows_activity_quick_start_without_a_gym_quick_start()
    {
        var state = DashboardStateBuilder.Build(
            new DateTime(2026, 9, 9),
            [new DashboardTemplateSummary(Guid.NewGuid(), "Push")],
            [],
            []);

        Assert.Equal("Activity", state.NextSessionName);
        Assert.True(state.ShowActivityQuickStart);
        Assert.False(state.ShowGymQuickStart);
    }

    [Fact]
    public void Friday_shows_the_next_planned_session_and_no_quick_start_for_rest()
    {
        var state = DashboardStateBuilder.Build(
            new DateTime(2026, 9, 11),
            [new DashboardTemplateSummary(Guid.NewGuid(), "Full Body")],
            [],
            []);

        Assert.Equal("Full Body", state.NextSessionName);
        Assert.Equal("Saturday", state.NextSessionDay);
        Assert.False(state.ShowGymQuickStart);
        Assert.False(state.ShowActivityQuickStart);
    }

    [Fact]
    public void Recent_workouts_and_activities_create_a_populated_summary()
    {
        var state = DashboardStateBuilder.Build(
            new DateTime(2026, 9, 7),
            [new DashboardTemplateSummary(Guid.NewGuid(), "Push")],
            [new DashboardWorkoutSummary("Push", new DateTime(2026, 9, 6))],
            [new DashboardActivitySummary(ActivityType.Walking, new DateTime(2026, 9, 5), 30)]);

        Assert.False(state.IsEmptyState);
        Assert.Equal(2, state.RecentItems.Count);
        Assert.Contains("1 workout", state.TrainingSummary);
        Assert.Contains("1 activity", state.TrainingSummary);
    }

    [Fact]
    public void No_history_shows_the_empty_state()
    {
        var state = DashboardStateBuilder.Build(
            new DateTime(2026, 9, 7),
            [new DashboardTemplateSummary(Guid.NewGuid(), "Push")],
            [],
            []);

        Assert.True(state.IsEmptyState);
        Assert.Empty(state.RecentItems);
    }
}
