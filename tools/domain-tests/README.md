# Domain tests without Unity

These projects compile the **same source files** used by Unity's `Fives.Domain`
and `Fives.Domain.Tests` assemblies. There are no engine stubs or copied rules.
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

## Board coverage

- Square (2×2 to 6×6) and rectangular (4×3, 3×4) fully filled swap boards.
- Imported permutations are validated and copied, so callers cannot mutate the board.
- Invalid swaps, row boundaries (including the last column of a 4×3 row), four-way adjacency, exact exchanges, and self-inverse swaps.
- 8,000 reproducible attempted swaps, checking permutation invariants after each attempt.
- Exhaustive traversal reaching all 24 arrangements of a 2x2 board.

- Five fixed shuffle vectors (four square, one 4×3) protect algorithm stability, including zero and negative seeds.
- 5,005 seed/dimension combinations are deterministic permutations with no tile in its own cell.
- Hint routes: closest misplaced tiles, shortest routes that avoid placed tiles, and following
  a route places its tile while moving others by one cell at most (300 seeds per size).

There are currently 75 standalone NUnit cases.

The fixtures also remain available in Unity's EditMode Test Runner through the
existing asmdef. Passing standalone tests does not imply they have run in Editor.
