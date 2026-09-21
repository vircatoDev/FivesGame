#!/usr/bin/env python3
"""Compile and run isolated correctness checks against current project sources."""
import json
import shutil
import subprocess
import tempfile
from pathlib import Path

project = Path(__file__).resolve().parents[2]
unity = Path("/Applications/Unity/Hub/Editor/6000.0.71f1/Unity.app/Contents")
source = project / "Assets/Scripts"
domain = project / "Assets/Fives/Domain"
probe = Path(__file__).resolve().parent
ecs_candidates = sorted((project / "Library/PackageCache").glob("com.leopotam.ecs@*"))
if not ecs_candidates:
    raise SystemExit("LeoECS package is missing from Library/PackageCache. Open the project in Unity once.")

runtime = unity / "NetCoreRuntime"
dotnet = runtime / "dotnet"
shared = runtime / "shared/Microsoft.NETCore.App/6.0.21"
files = [probe / "EngineStubs.cs", probe / "CorrectnessProbes.cs", probe / "LifecycleProbes.cs"]
files += list(domain.glob("*.cs"))
files += list((ecs_candidates[-1] / "src").glob("*.cs"))
for directory in ["Configs", "Models", "Components", "Commands", "Services/Interfaces"]:
    files += list((source / directory).glob("*.cs"))
for name in [
    "Services/GameStartService", "Services/StorageService", "Services/SoundService",
    "Services/EnergyService", "Services/StarService", "Services/PlayerProgressService",
    "Services/ECSCommandService", "Helpers/PlayerDataSaveHelper", "Helpers/PuzzleGenerator",
    "UI/Presenters/BasePresenter", "UI/Presenters/SelectMenuPresenter", "UI/Presenters/GameResultPresenter",
    "UI/Presenters/MainMenuPresenter", "UI/TileUiProvider", "Systems/CommonUIHeaderPanelSystem",
    "Systems/WinCheckSystem", "Systems/EnergyRecoverySystem", "Systems/StorageSystem",
    "Systems/TileClickSystem", "Systems/TileMoveSystem"
]:
    files.append(source / f"{name}.cs")

with tempfile.TemporaryDirectory(prefix="fives-probes-") as temporary:
    directory = Path(temporary)
    output = directory / "Probes.dll"
    response = directory / "compile.rsp"
    options = ["-nologo", "-target:exe", "-langversion:9", f"-out:{output}"]
    options += [f"-r:{assembly}" for assembly in shared.glob("*.dll")]
    newtonsoft = next((project / "Library/PackageCache").glob("com.unity.nuget.newtonsoft-json@*/Runtime/Newtonsoft.Json.dll"))
    options.append(f"-r:{newtonsoft}")
    shutil.copy2(newtonsoft, directory / "Newtonsoft.Json.dll")
    options += [str(file) for file in files]
    response.write_text("\n".join(f'"{option}"' for option in options))
    subprocess.run([str(dotnet), str(unity / "DotNetSdkRoslyn/csc.dll"), f"@{response}"], check=True, timeout=60)
    (directory / "Probes.runtimeconfig.json").write_text(json.dumps({
        "runtimeOptions": {"tfm": "net6.0", "framework": {"name": "Microsoft.NETCore.App", "version": "6.0.21"}}
    }))
    subprocess.run([str(dotnet), str(output)], check=True, timeout=30)
