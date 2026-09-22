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

The existing `python3 tools/logic-probes/run.py` remains the separate suite for
legacy services/presenters/ECS with engine substitutes; it requires Unity's local
compiler and resolved packages.

## Board coverage

- Sizes 2, 3, 4, and 6; any image tile can be the hidden tile.
- Imported permutations are validated and copied, so callers cannot mutate the board.
- Invalid moves, row boundaries, four-way adjacency, exact swaps, and reverse moves.
- 8,000 reproducible attempted moves, checking permutation and move invariants after
  each attempt.
- Exhaustive traversal of all 12 reachable 2x2 arrangements for each hidden tile ID.

- Four fixed shuffle vectors protect algorithm stability, including zero and negative seeds.
- 6,565 seed/size/hidden-ID combinations preserve permutation, determinism and a non-solved start.
- Seeded 2x2 layouts are checked against the complete reachable state set.
- Replay reconstructs each move; invalid/version-mismatched data is rejected and histories are copied.

There are currently 74 standalone NUnit cases. The 50 local service/ECS probes
also cover Undo, replay, interruption, cleanup/restart and animation/input ordering.
Those probes use engine substitutes and are not in the GitHub-hosted domain workflow.

The fixtures also remain available in Unity's EditMode Test Runner through the
existing asmdef. Passing standalone tests does not imply they have run in Editor.
