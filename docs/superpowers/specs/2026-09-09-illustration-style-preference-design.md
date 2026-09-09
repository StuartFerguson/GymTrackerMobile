# Illustration Style Preference

## Goal

Let users choose which illustration style is shown for workout and activity artwork, with Female, Male, and Neutral options. The choice must persist locally and be shared consistently by Start Workout and Weekly Plan.

## Design

Add an `IllustrationStyle` value with `Female`, `Male`, and `Neutral` options. Store the selected value through the existing `ISettingsRepository` using a single setting key and default to `Neutral` when no valid value is stored.

Centralize filename selection in a UI illustration resolver. Each logical artwork key (Push, Pull, Legs, Full Body, Walk, Swim, Rest) resolves to the selected style's local PNG asset. The existing page view models pass the resolved filename into their presentation state; pages do not construct filenames themselves.

The Start Workout screen exposes the selector as a compact control near the header so the feature is immediately testable. Weekly Plan consumes the same persisted preference when it loads. The selector is presentation-only and does not change workout or activity behavior.

## Assets

Provide three cohesive local asset families: female, male, and neutral/abstract. Each family includes Push, Pull, Legs, Full Body, Walk, Swim, and Rest. Raster PNGs are used for the person-focused families to preserve the polished shaded illustration style of the mockup; neutral artwork may use abstract activity/equipment illustrations.

## Error handling

If preference loading fails, use Neutral for the current session. If saving fails, retain the selected style in memory and show a recoverable error without disrupting the current screen.

## Testing

- settings repository stores and retrieves the preference key
- invalid or missing values default to Neutral
- each style resolves all seven logical artwork keys
- switching the style updates the current screen's artwork mapping
- Weekly Plan and Start Workout use the same resolver
- preference-save failures do not lose the in-memory selection
