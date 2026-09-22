# Unity 6 Android baseline

## Supported environment

- Unity Editor: `6000.0.71f1` (`907bc2d768b5`).
- Target: Android only.
- Store artifact: Android App Bundle (`.aab`).
- Scripting backend: IL2CPP.
- Architecture: ARM64.
- Minimum Android version: Android 6.0 (API 23), the Unity 6 player minimum.
- Target API: highest installed SDK selected automatically.
- Application identifier: `com.vircatodev.fivesgame`.

Android Build Support, SDK, NDK, and OpenJDK are installed with the editor. The enabled scene is `Assets/Scenes/MainGame.unity`.

## Reproducibility work

- Unity registry packages were migrated to versions resolved by Unity 6.
- Unused Ads, Analytics, IAP, Visual Scripting, Timeline, XR helpers, Collab, and feature bundles were removed.
- Git packages are pinned to full commit hashes in `Packages/manifest.json`.
- Newtonsoft.Json now uses Unity's maintained registry package.
- The package lock was regenerated after the manifest cleanup.
- `Fives.Editor.AndroidBuild.BuildRelease` provides one deterministic Android AAB entry point.
- Media without recorded provenance is listed in `THIRD_PARTY_NOTICES.md` and blocks a public store release until reviewed or replaced.

## Verification performed

- Package Manager resolved the cleaned manifest and regenerated `Packages/packages-lock.json`.
- `AndroidBuild.cs` compiles against the Unity 6 editor API.
- The `Fives.Domain` assembly and its NUnit EditMode test assembly compile against Unity's managed profile.
- `tools/logic-probes/run.py` compiles current production sources and passes all 34 correctness probes (including lifecycle and save recovery regressions).
- Unity Roslyn compiles the complete `Assembly-CSharp` source set after the lifecycle fixes.

## Sandbox limitation

The Codex terminal sandbox prevents Unity's IL post-processor from creating its Unix socket under `/tmp`. Unity therefore stops before its normal script compilation stage with:

```text
System.InvalidOperationException: Can't find file /tmp/ilpp.sock-...
```

This is an execution-environment limitation. Complete the remaining checks from Unity Hub or an unrestricted terminal on the same machine.

## Commands

Run isolated correctness probes:

```bash
tools/logic-probes/run.py
```

Run Unity EditMode tests:

```bash
/Applications/Unity/Hub/Editor/6000.0.71f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "$PWD" \
  -runTests -testPlatform EditMode \
  -testResults Logs/editmode-results.xml
```

Build the Android App Bundle:

```bash
/Applications/Unity/Hub/Editor/6000.0.71f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "$PWD" \
  -buildTarget Android \
  -executeMethod Fives.Editor.AndroidBuild.BuildRelease \
  -buildPath Builds/Android/FivesGame.aab \
  -logFile Logs/android-build.log
```

Store signing is intentionally not committed. Configure a private upload keystore before publishing to Google Play.


## Review-fix verification

The isolated probes execute production services, presenters and ECS systems with small engine/UI substitutes. They cover repeated start, input during movement/win, manual exit, cold-start energy, corrupt/partial JSON, SFX volume and the tile sprite destruction callback. Newtonsoft.Json and LeoECS are real project dependencies; PlayerPrefs, rendering, audio playback and Unity object destruction are substitutes. These checks do not measure native memory or prove Android behaviour.

The complete runtime source set also compiles with Unity 6000.0.71f1 Roslyn and the generated project references. On 2026-09-21, the Unity CLI EditMode run was blocked because this project was already open in Editor (PID 38148). No Unity Test Runner success or Android build is claimed for these changes.
