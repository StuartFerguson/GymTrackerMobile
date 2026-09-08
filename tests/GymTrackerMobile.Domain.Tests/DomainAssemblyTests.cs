namespace GymTrackerMobile.Domain.Tests;

public sealed class DomainAssemblyTests
{
    [Fact]
    public void Domain_assembly_has_expected_identity()
    {
        Assert.Equal("GymTrackerMobile.Domain", typeof(GymTrackerMobile.Domain.DomainMarker).Assembly.GetName().Name);
    }
}
