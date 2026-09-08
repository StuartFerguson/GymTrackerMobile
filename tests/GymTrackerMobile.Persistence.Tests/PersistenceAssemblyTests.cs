namespace GymTrackerMobile.Persistence.Tests;

public sealed class PersistenceAssemblyTests
{
    [Fact]
    public void Persistence_assembly_has_expected_identity()
    {
        Assert.Equal("GymTrackerMobile.Persistence", typeof(GymTrackerMobile.Persistence.PersistenceMarker).Assembly.GetName().Name);
    }
}
