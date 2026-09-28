# Domain tests without Unity

These projects compile the **same source files** used by Unity's `Fives.Domain`
and `Fives.Domain.Tests` assemblies (`Assets/Fives/Domain` and `Assets/Tests/Domain`). There are no engine stubs or copied rules.
The domain targets .NET Standard 2.1 and C# 9, with no Unity references or packages.
The standalone NUnit runner targets .NET 8; it does not change the game's runtime.

With a .NET 8 SDK installed, run from the repository root:

```sh
dotnet restore tools/domain-tests/Tests/Fives.Domain.Tests.csproj --locked-mode
dotnet test tools/domain-tests/Tests/Fives.Domain.Tests.csproj --no-restore --configuration Release --logger "trx;LogFileName=domain.trx" --results-directory tools/domain-tests/TestResults
```

The GitHub Actions workflow runs these commands on Linux and Windows and retains
the TRX reports, including on test failure. It needs no Unity license or secrets.
These are domain checks, not PlayMode, rendering, or Android build verification.

Runtime behaviour (ECS systems, services, presenters, saves) is covered by the
`Fives.Runtime.Tests` EditMode assembly, which runs in Unity's Test Runner only.

## Coverage

All board checks use the game's 4×3 board.

- Imported permutations are validated and copied, so callers cannot mutate the board.
- Invalid swaps, row boundaries (the last cell of a row is not next to the first cell of the next one), four-way adjacency, exact and self-inverse swaps.
- 1,000 reproducible attempted swaps, checking permutation invariants after each attempt.
- A fixed shuffle vector protects the algorithm's stability; 1,001 seeds give deterministic permutations with no tile in its own cell.
- Hint routes: closest misplaced tiles, shortest routes that avoid placed tiles, and following a route places its tile while moving others by one cell at most (300 seeds).
- Wallet rules, energy recovery with its remainder, a reward claimed once, theme progress and the save migration from names to ids.

There are currently 33 standalone NUnit cases. [The test map](../../docs/TESTS.md) describes every test group.

The fixtures also remain available in Unity's EditMode Test Runner through the
existing asmdef. Passing standalone tests does not imply they have run in Editor.
