using Scripts.Helpers;
using System.Linq;
using System;
using System.Collections.Generic;
using Leopotam.Ecs;
using Fives.Domain;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.Systems;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
internal static partial class CorrectnessProbes
{
    private static void CheckResultProgress()
    {
        var config = CreateConfig();
        var theme = config.Themes[1];
        theme.Puzzles = new[] { "a", "b", "c" }.Select(id => new PuzzleData { Id = id, Name = id }).ToArray();
        var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
        var session = new GameSession(config);
        session.SetSelectedTheme(theme); session.SetSelectedImage(theme.Puzzles[2]); session.BeginRun();
        var world = new EcsWorld();
        var view = new GameResultView();
        new GameResultPresenter(session, new StarService(save), new PlayerProgressService(save), world).Initialize(view);
        Check("result shows solved puzzles, not the puzzle's position", view.Progress == "1/3", view.Progress);
        world.Destroy();
    }

    private static void CheckPresenterAllocation()
    {
        var world = new EcsWorld();
        var session = new GameSession(CreateConfig()); session.BeginRun();
        var entity = world.NewEntity();
        entity.Replace(new BoardComponent { State = SeededShuffle.Create(3, 42) });
        entity.Replace(new BoardHistoryComponent { Seed = 42, Moves = new List<Swap>(), Undone = new List<Swap>() });
        var presenter = new GamePlayPresenter(session, world);
        var view = new GamePlayView();
        SetView(presenter, view);
        var systems = new EcsSystems(world).Add(new BoardHudSystem(presenter)).Inject(session);
        systems.Init();
        for (var i = 0; i < 1000; i++) systems.Run();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++) systems.Run();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Check("idle HUD does not touch the view or allocate", bytes == 0 && view.Updates == 1,
            $"{bytes} bytes, {view.Updates} view updates / 11000 frames");
        entity.Get<BoardHistoryComponent>().Moves.Add(new Swap(0, 1));
        systems.Run();
        Check("HUD updates when history changes", view.Updates == 2 && view.Status == "Moves: 1", view.Status);
        systems.Destroy(); world.Destroy();
    }
}
namespace Scripts.UI.Views
{
    public class GamePlayView : BaseView
    {
        public void UpdateViewContent(PuzzleData puzzle) { }
        public Cysharp.Threading.Tasks.UniTask PlayShowAnimation() => default;
        public int Updates;
        public string Status = "";
        public void UpdateControls(string moves, bool canUndo, bool canRedo) { Updates++; Status = moves; }
    }
}
