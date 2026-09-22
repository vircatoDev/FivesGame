# Gameplay screen verification

`Assets/Tests/EditorUI/GamePlayPrefabTests.cs` contains four prefab-reference cases
and one Unity lifecycle scenario. The scenario creates the real screen twice,
checks the initial controls, updates move/replay text and button states, toggles
visibility, and destroys the screen. Unexpected Unity exceptions fail the test.
It does not test ECS command routing, victory/rewards, device input or rendering quality.

In Unity 6000.0.71f1, stop Play Mode, open Window > General > Test Runner,
choose EditMode, filter `GamePlayPrefabTests` and run the selected tests.
The lifecycle scenario enters and exits Play Mode itself.

With the project closed in Editor, the equivalent CLI command is:

```sh
unity test . --mode EditMode --filter Fives.UI.Tests.GamePlayPrefabTests --output /tmp/fives-ui-tests.xml --timeout 300
```

Status on 2026-09-22: the test assembly compiles against Unity 6000.0.71f1.
The isolated Editor attempt timed out after 60 seconds and produced no test report;
a passing runtime result is not claimed. Further attempts were deferred to conserve
usage at the owner's request. Next: run these tests in the regular Editor, then
verify menu → puzzle → Undo/replay → victory → reward → restart and Android separately.
