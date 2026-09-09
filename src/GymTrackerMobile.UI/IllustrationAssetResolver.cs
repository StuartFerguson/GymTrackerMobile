namespace GymTrackerMobile.UI;

public static class IllustrationAssetResolver
{
    public static string Resolve(IllustrationStyle style, IllustrationAssetKey key)
    {
        var prefix = style switch
        {
            IllustrationStyle.Female => "",
            IllustrationStyle.Male => "male_",
            _ => "neutral_"
        };

        var assetName = key switch
        {
            IllustrationAssetKey.Push => "workout_push.png",
            IllustrationAssetKey.Pull => "workout_pull.png",
            IllustrationAssetKey.Legs => "workout_legs.png",
            IllustrationAssetKey.FullBody => "workout_full_body.png",
            IllustrationAssetKey.Walk => "activity_walk.png",
            IllustrationAssetKey.Swim => "activity_swim.png",
            IllustrationAssetKey.Rest => "activity_rest.png",
            _ => "workout_full_body.png"
        };

        return prefix + assetName;
    }
}
