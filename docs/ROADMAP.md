# Modernization roadmap

## 0. Reproducible baseline

- Install Unity 2021.3.25f1 and restore locked packages.
- Exercise menu → puzzle → win → reward → restart.
- Capture Editor/build logs and a Build Report.
- Verify asset redistribution rights and add LICENSE/THIRD_PARTY_NOTICES.
- Pin Git dependencies explicitly in the manifest.

Exit: a clean clone opens and builds, or the smallest compatibility fix is documented.

## 1. Correctness

- Add regressions for purchase, reward, recovery, event ordering, and rapid tile input.
- Make purchases and rewards atomic and idempotent.
- Introduce a clock abstraction and correct recovery remainder handling.
- Add cancellation and serialized navigation transitions.
- Handle corrupt saves and disable unsupported 6×6 until generic rules exist.

Exit: the eight reproduced defects have passing Unity tests.

## 2. Deterministic domain

- Add assembly boundaries and pure `BoardState`.
- Support tested 3×3 and 4×4 rules without hardcoded width.
- Add Undo, replay format, seeded shuffle, and session results.
- Run property-style tests over many seeds and move sequences.
- Record the ECS-to-domain decision in an ADR.

Exit: replay reconstructs the final board hash without MonoBehaviour.

## 3. Persistence and content

- Add versioned saves, migrations, stable IDs, backup, and recovery.
- Move screens and themes to Addressables with ownership tests.
- Remove copied runtime textures and profile repeated sessions.
- Remove unused samples and packages.
- Add a UI Toolkit catalog validator.

Exit: content is data-driven and repeated loading does not grow live resources.

## 4. Portfolio gameplay

- Replay viewer and share code.
- Daily challenge and streak.
- A* hints for 3×3 and bounded hints for 4×4.
- Move/time challenge rules and achievements.
- Typed local telemetry with a debug overlay.

Exit: every feature has tests, a documented trade-off, and a short demonstration.

## 5. Delivery quality

- RU/EN Localization.
- Touch, pointer, keyboard, and gamepad Input System actions.
- Focus navigation, safe area, high contrast, and reduced motion.
- Independent AudioMixer controls for music and SFX.
- Suspend/resume and active-session recovery.
- GitHub Actions/GameCI, WebGL and desktop artifacts, profiler report, and release video.

Exit: the full flow is reproducible in CI and available as a playable build.
