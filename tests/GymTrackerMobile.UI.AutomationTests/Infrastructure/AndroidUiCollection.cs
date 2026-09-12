using Xunit;

namespace GymTrackerMobile.UI.AutomationTests.Infrastructure;

[CollectionDefinition("Android UI", DisableParallelization = true)]
public sealed class AndroidUiCollection : ICollectionFixture<AndroidDriverFixture>
{
}
