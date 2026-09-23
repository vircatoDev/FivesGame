using System;
using System.Collections.Generic;
using Leopotam.Ecs;
using Fives.Domain;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
internal static partial class CorrectnessProbes
{
    private static void CheckPresenterAllocation()
    {
        var world = new EcsWorld();
        var session = new GameSession(CreateConfig()); session.BeginRun();
        var entity = world.NewEntity();
        entity.Replace(new BoardComponent { State = SeededShuffle.Create(3, 8, 42, 36) });
        entity.Replace(new BoardHistoryComponent { Seed = 42, Moves = new List<int>() });
        var presenter = new GamePlayPresenter(session, world);
        SetView(presenter, new GamePlayView());
        for (var i = 0; i < 1000; i++) presenter.RefreshControls();
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++) presenter.RefreshControls();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Check("idle presenter does not allocate status strings", bytes == 0, $"{bytes} bytes / 10000 calls (stub view)");
        world.Destroy();
    }
}
namespace Scripts.UI.Views
{
    public class GamePlayView : BaseView
    {
        public void UpdateViewContent(PuzzleData puzzle) { }
        public Cysharp.Threading.Tasks.UniTask PlayShowAnimation() => default;
        public void UpdateControls(string status, bool canUndo, bool canReplay, bool replaying) { }
    }
}
