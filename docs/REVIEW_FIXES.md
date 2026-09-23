# Review fixes before deterministic board work

Scope: findings 1–6 from the 2026-09-21 review (including shared texture ownership and SFX volume in item 6). These changes extend commit `07c374f`; no new architecture assemblies are introduced.

## Ownership and reuse

- `GameStartService` is the only menu operation that spends energy and starts a run. The existing VContainer installer registers it once. Both presenters delegate to it.
- `GameSession.IsRunning` prevents a second start until cleanup ends the run. `IsCompleted` locks moves during the result presentation and replaces WinCheckSystem's private victory flag. Beginning a new run resets completion.
- `MoveComponent` owns its animation start, elapsed time and duration. One ECS update loop advances movement; the presence of the component also blocks further input. The unused direction field is removed.
- `EnergyRecoverySystem.Init` recovers offline energy before the header is initialized. The same recovery method handles later updates. The header gets its initial balances from the live services.
- `StorageService` handles malformed/null JSON once at the storage boundary, preserving its raw value under `<key>.corrupt`. `PlayerDataSaveHelper` supplies missing model fields without resetting valid progress. Unknown content IDs are retained; the menu selects an available theme.
- `BoardInitSystem` creates Sprite regions over the original texture. `TileUiProvider` destroys the Sprite it owns; it never destroys the shared source texture. The texture-copying `ImageSplitter` is removed.

## Repeat review

The changes were re-read after the first regression run, checking call sites, DI registration, ECS order, run restart, rapid input and resource ownership. Two additional cases were corrected during that pass: manual exit during the victory delay and a save referencing a removed theme.

No further blocking issue was found in these specific fixes by source review and the isolated checks. This is not a claim that the whole game is bug-free or that the architecture migration is ready for release.

Validation:

- 34 isolated correctness probes pass, including the original eight checks.
- Full runtime source compilation succeeds with Unity 6000.0.71f1 Roslyn. Existing unawaited-view-animation warnings remain outside these changes.
- `git diff --check` passes.
- EditMode execution is blocked by the running Editor. PlayMode, Android build and native-memory profiling remain unverified.

Tiles no longer own sprites: each `RawImage` shows its `uvRect` of the shared puzzle texture, so board creation allocates no textures or sprites and there is nothing to destroy.

## Editor checks to complete

1. Rapidly press Start in each menu: one energy unit is charged and one game opens.
2. Rapidly reverse a moving tile; solve the puzzle and tap during the two-second result display: no overlapping motion or post-win move.
3. Exit during movement and at the end of the win delay; restart several rounds: no stale result and no stuck input.
4. Cold-start after offline energy recovery: header, live balance and saved balance agree.
5. Verify sprite orientation/cropping and native object counts across repeated rounds; check partial/malformed saves on a test profile.
6. Mute/change SFX and verify actual device playback, then run the Android IL2CPP build.

Navigation teardown, full save migrations, broader removal of command wrappers, and the deterministic-board feature remain separate work. No semaphore, cancellation token, RunId or pending board-destruction queue is added here.
