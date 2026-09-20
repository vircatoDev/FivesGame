# Contributing

FivesGame follows a lightweight Git Flow process.

## Branches

Create features from `develop`:

```sh
git switch develop
git pull --ff-only
git switch -c feature/short-description
```

Use `fix/*` for ordinary bug fixes, `release/x.y.z` for stabilization, and `hotfix/x.y.z` for urgent fixes from `main`. Changes reach `main` and `develop` through pull requests once branch protection is configured.

## Commits

Use focused Conventional Commits:

```text
fix: prevent duplicate theme debit
test: cover concurrent tile input
feat: add deterministic replay format
docs: record save migration decision
```

Do not combine package upgrades, mass formatting, scene serialization, and gameplay refactoring in one commit. Include Unity `.meta` files with their assets. Never commit `Library`, credentials, keystores, Unity license files, or service secrets.

## Pull requests

Describe the concrete problem, resulting behavior, architecture or migration effects, and validation performed. Attach screenshots or video for UI work and profiler evidence for performance claims. Confirm redistribution rights for new assets and dependencies.

## Verification

Run the applicable checks:

1. EditMode tests for domain and persistence changes.
2. PlayMode tests for navigation, lifecycle, input, and content loading.
3. A complete menu → puzzle → result → menu smoke test.
4. A target-platform build for package, serialization, or platform changes.

CI will later enforce tests and publish build artifacts. Coverage protects critical behavior; it is not a percentage target by itself.

## Releases

Merge a tested `release/x.y.z` into `main`, tag `vX.Y.Z`, and merge it back into `develop`. Apply the same back-merge rule to hotfixes. Do not force-push protected branches.
