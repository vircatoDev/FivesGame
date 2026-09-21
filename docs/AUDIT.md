# Imported baseline audit

Audit date: 2026-09-20. Imported source commit: `36d201a8985047560a0b55b2bc5a1c56ea60b777`.

The baseline is a useful portfolio foundation: it includes a complete player loop, ScriptableObject configuration, dependency injection, separate gameplay systems, and locked package revisions. Its main weakness is that several important operations are not atomic and some lifecycles depend on ECS system order or unowned asynchronous delays.

## Reproduced defects

Isolated C# probes using the original application code and the LeoECS revision from `packages-lock.json` reproduced eight problems:

1. One theme purchase can debit the price twice.
2. A negative `Spend` value increases currency.
3. Reward claims are not idempotent.
4. Offline energy recovery changes the balance without emitting the UI update.
5. Fractional recovery time is discarded.
6. A one-frame save event emitted after `StorageSystem` disappears before consumption.
7. Two tile clicks in one ECS tick can place two tiles in the same cell.
8. The generator always creates nine cells although a 6×6 configuration exists.

## Resolution status

The correctness branch addresses all eight defects:

1. theme purchase spends once and persists both currency and progress;
2. currency and energy reject non-positive spends;
3. a per-run claim token makes reward collection idempotent;
4. recovery explicitly returns its amount and emits one UI update;
5. recovery advances by whole intervals and preserves fractional elapsed time;
6. save requests persist until `StorageSystem` consumes and destroys them;
7. the input system accepts at most one tile click while a move is pending;
8. generation and win checks use the configured board size.

`tools/logic-probes/run.py` compiles the current production sources with the locked LeoECS revision and reports `8/8 correctness probes passed`. Five additional NUnit EditMode tests protect the pure domain rules.

State transitions now run through one small latest-request queue on Unity's main thread. The win presentation delay is an ordinary ECS update, and board cleanup runs immediately when it receives `GameEndEvent`; neither starts unowned async work. DOTween animations are linked to their owning GameObjects and stop when those objects are destroyed.

## Risks requiring Unity verification

- runtime Texture2D/Sprite instances have no explicit ownership;
- save JSON has no schema, migration, backup, or stable content identifiers;
- SFX volume settings are stored but not applied to effect playback;
- timed mode and session results are incomplete;
- reflection-created commands require a real IL2CPP/stripping check.

## Unity 6 migration status

The imported source targeted Unity 2021.3.25f1. The maintained baseline now targets Unity 6000.0.71f1 and Android. The first headless import resolved the Unity license and started package migration, but the Codex sandbox blocks Unity's IL post-processor Unix socket under `/tmp`. Editor compilation, PlayMode, Android IL2CPP, FPS, GC allocations, and native memory therefore still require an unsandboxed Unity run.

The remaining verification step is to execute the committed EditMode suite and Android build from an unsandboxed Unity process.
