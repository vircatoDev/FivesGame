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

## Risks requiring Unity verification

- state transitions can overlap because async work has no cancellation owner;
- delayed win and board-destruction work can affect a later session;
- runtime Texture2D/Sprite instances have no explicit ownership;
- save JSON has no schema, migration, backup, or stable content identifiers;
- SFX volume settings are stored but not applied to effect playback;
- timed mode and session results are incomplete;
- reflection-created commands require a real IL2CPP/stripping check.

## Limits

The source targets Unity 2021.3.25f1. The audit machine currently has Unity 6000.0.71f1, so the imported baseline has not yet been opened or rewritten. Editor compilation, PlayMode, IL2CPP, FPS, GC allocations, and native memory remain unverified.

The first milestone converts these findings into Unity Test Framework regressions and fixes correctness before adding new systems.
