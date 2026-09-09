namespace GymTrackerMobile.UI.Tests;

public sealed class IllustrationAssetResolverTests
{
    public static IEnumerable<object[]> Cases() =>
        from style in new[] { IllustrationStyle.Female, IllustrationStyle.Male, IllustrationStyle.Neutral }
        from key in Enum.GetValues<IllustrationAssetKey>()
        select new object[] { style, key };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Resolve_returns_a_local_png_for_each_style_and_key(IllustrationStyle style, IllustrationAssetKey key)
    {
        var asset = IllustrationAssetResolver.Resolve(style, key);

        Assert.False(string.IsNullOrWhiteSpace(asset));
        Assert.EndsWith(".png", asset, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_uses_neutral_assets_for_unknown_style_values()
    {
        var asset = IllustrationAssetResolver.Resolve((IllustrationStyle)999, IllustrationAssetKey.Push);

        Assert.Equal(
            IllustrationAssetResolver.Resolve(IllustrationStyle.Neutral, IllustrationAssetKey.Push),
            asset);
    }

    [Fact]
    public void Resolve_returns_unique_assets_for_each_style_and_key()
    {
        var assets = Cases()
            .Select(values => IllustrationAssetResolver.Resolve((IllustrationStyle)values[0], (IllustrationAssetKey)values[1]))
            .ToList();

        Assert.Equal(assets.Count, assets.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
