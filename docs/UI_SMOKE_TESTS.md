# Gameplay screen verification

`Assets/Tests/EditorUI/GamePlayPrefabTests.cs` contains four prefab-reference cases
and one Unity lifecycle scenario. The scenario creates the real screen twice,
checks the initial controls, updates the move text and Undo/Hint states, toggles
visibility, and destroys the screen. Unexpected Unity exceptions fail the test.
It does not test ECS command routing, victory/rewards, device input or rendering quality.

In Unity 6000.0.71f1, stop Play Mode, open Window > General > Test Runner,
choose EditMode, filter `GamePlayPrefabTests` and run the selected tests.
The lifecycle scenario enters and exits Play Mode itself.

With the project closed in Editor, the equivalent CLI command is:

```sh
unity test . --mode EditMode --filter Fives.UI.Tests.GamePlayPrefabTests --output /tmp/fives-ui-tests.xml --timeout 300
```

Status on 2026-09-25: the tests are typed against `Fives.Runtime` and pass in the regular
Editor together with the domain and runtime EditMode suites (158 tests).

## Fast CI format guard

`python3 tools/check_unity_text.py` scans text prefabs and scenes under Assets.
It rejects blank/whitespace-only lines, non-text files and an empty input directory.
The separate `Unity prefab and scene format` Actions job runs without Unity.
This catches the specific serialization regression that lost the BoardControlsView
references. It does not parse all UnityYAML syntax, resolve references, or replace
Editor/PlayMode tests. Unity documents this format limitation here:
https://unity.com/blog/engine-platform/understanding-unitys-serialization-language-yaml
