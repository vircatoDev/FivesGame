# Rewarded ads

The result screen offers **x2**: the player watches a rewarded ad and gets double stars. Ads go through
[Unity LevelPlay](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/rewarded-ad-integration-package) with the
Unity Ads network, on Android only.

## Children

The game is for ages 8–14, and every player is treated as a child (COPPA, Google Play Families policy). Before the
SDK starts, `LevelPlayRewardedAds` sets:

- `LevelPlayPrivacySettings.SetCOPPA(true)`: the app is child-directed;
- `is_deviceid_optout = true`: the advertising ID is not used;
- `UnityAds_coppa = true`: Unity Ads serves contextual ads only.

`ChildDirectedManifest` removes the `AD_ID` permission from the merged Android manifest, whichever library declares
it. Unity Ads and ironSource are self-certified ads SDKs for Google Play Families apps.

## Where ads show

| Where | Ads | x2 button |
| --- | --- | --- |
| Editor, Android platform | LevelPlay's mock ads, no account needed | shown when the mock ad is loaded |
| Android build with keys | real ads | shown when an ad is loaded |
| Android build without keys | none | hidden |
| WebGL | none (`NoRewardedAds`) | hidden |

The stars double only after the ad reports its reward; a skipped or failed ad grants nothing, and the plain reward
stays available.

## Trying it without an account

Switch the editor to the Android platform and enter play mode. With empty keys, the editor uses mock keys and
LevelPlay's mock rewarded ad, so the whole x2 flow runs.

## Real ads on a device

1. Register the app in the [LevelPlay dashboard](https://levelplay.io/login): Android, package
   `com.vircatodev.fivesgame`, directed to children; add a Rewarded ad unit.
2. Put the **App Key** and the rewarded **Ad Unit ID** into `Assets/Configs/GameConfig.asset`, section
   **Ads (LevelPlay, Android)**.
3. In Unity: import the Mobile Dependency Resolver when asked, install **LevelPlay Mediation** in
   **Ads Mediation > Network Manager** (it bundles Unity Ads), then
   **Assets > Mobile Dependency Resolver > Android Resolver > Resolve**.
4. In the Unity Cloud Monetization settings, mark the game as directed to children and turn on the Google Designed for
   Families flag.
