using System;
using System.Linq;
using Fives.Domain;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
using Scripts.Models;
using Scripts.Systems;

namespace Fives.Runtime.Tests
{
    public sealed class BoardProjectionTests
    {
        private BoardFixture _board;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardFixture().WithSeededBoard(42);
            _board.Systems = new EcsSystems(_board.World).Add(new BoardProjectionSystem())
                .OneFrame<BoardChangedEvent>().OneFrame<BoardInitializedEvent>().Inject(_board.Session);
            _board.Systems.Init();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        [Test]
        public void IdleFrame_LeavesTilesUntouched()
        {
            _board.Tiles[0].Get<TileComponent>().Cell = -1; // A sentinel the projection would overwrite.
            _board.Systems.Run();

            Assert.That(_board.Tiles[0].Get<TileComponent>().Cell, Is.EqualTo(-1));
            Assert.That(_board.Tiles[0].Has<MoveComponent>(), Is.False);
        }

        [Test]
        public void InitialProjection_PlacesEveryTileInstantly()
        {
            _board.World.NewEntity().Get<BoardInitializedEvent>();
            _board.Systems.Run();

            for (var cell = 0; cell < 9; cell++)
            {
                var tile = _board.Tiles[_board.Layout[cell]];
                Assert.That(tile.Get<TileComponent>().Cell, Is.EqualTo(cell));
                Assert.That(tile.Get<MoveComponent>().InstaMove, Is.True);
            }
        }

        [Test]
        public void BoardChange_AnimatesOnlyTheMovedTiles()
        {
            _board.Layout.TrySwap(new Swap(0, 1));
            _board.World.NewEntity().Get<BoardChangedEvent>();
            _board.Systems.Run();

            Assert.That(_board.Tiles[_board.Layout[0]].Get<MoveComponent>().InstaMove, Is.False);
            Assert.That(_board.Tiles[_board.Layout[1]].Has<MoveComponent>(), Is.True);
            Assert.That(Enumerable.Range(2, 7).All(cell => !_board.Tiles[_board.Layout[cell]].Has<MoveComponent>()), Is.True);
        }
    }

    public sealed class BoardSetupAndCleanupTests
    {
        private BoardFixture _board;
        private EcsFilter<BoardComponent, BoardHistoryComponent> _boards;

        [TearDown]
        public void TearDown() => _board.Dispose();

        private void Build(int size, bool withCleanup)
        {
            _board = new BoardFixture(size);
            _board.Systems = new EcsSystems(_board.World).Add(new BoardSetupSystem());
            if (withCleanup)
                _board.Systems.Add(new BoardDestroySystem(_board.Objects.Rect("Board parent"))).OneFrame<GameEndEvent>();
            _board.Systems.Inject(_board.Session).Inject(_board.Time);
            _board.Systems.Init();
            _boards = (EcsFilter<BoardComponent, BoardHistoryComponent>)_board.World.GetFilter(typeof(EcsFilter<BoardComponent, BoardHistoryComponent>));
        }

        [Test]
        public void Setup_CreatesOneReproducibleBoard()
        {
            Build(6, false);
            _board.Systems.Tick(2);

            Assert.That(_boards.GetEntitiesCount(), Is.EqualTo(1));
            var layout = _boards.Get1(0).State;
            var expected = SeededShuffle.Create(6, _boards.Get2(0).Seed);
            Assert.That(layout.CellCount, Is.EqualTo(36));
            Assert.That(layout.IsSolved, Is.False);
            Assert.That(Enumerable.Range(0, 36).All(cell => layout[cell] == expected[cell]), Is.True, "the seed recreates the layout");
        }

        [Test]
        public void Cleanup_RemovesBoardHistoryAndSelectionWithoutAView()
        {
            Build(3, true);
            _board.Systems.Run();
            _boards.GetEntity(0).Get<TileSelectionComponent>();
            _boards.Get2(0).Undone.Add(new Swap(0, 1));
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Run();

            Assert.That(_boards.GetEntitiesCount(), Is.Zero);
            Assert.That(_board.World.Count<TileSelectionComponent>(), Is.Zero);
            Assert.That(_board.Session.IsRunning, Is.False);
        }

        [Test]
        public void EndedRun_CannotRecreateABoard_UntilTheNextRun()
        {
            Build(3, true);
            _board.Systems.Run();
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Tick(2);
            Assert.That(_boards.GetEntitiesCount(), Is.Zero, "the Playing state may still be waiting for navigation");

            _board.Session.BeginRun();
            _board.Systems.Run();
            Assert.That(_boards.GetEntitiesCount(), Is.EqualTo(1));
            Assert.That(_boards.Get2(0).Moves, Is.Empty);
            Assert.That(_boards.Get2(0).Undone, Is.Empty);
            Assert.That(_boards.GetEntity(0).Has<TileSelectionComponent>(), Is.False);
        }
    }

    public sealed class WinFlowTests
    {
        private const int SolvingTile = 7; // Sits in cell 8; one swap left from solved.
        private BoardFixture _board;

        [SetUp]
        public void SetUp() => _board = new BoardFixture()
            .WithBoard(new BoardState(3, new[] { 0, 1, 2, 3, 4, 5, 6, 8, 7 }))
            .WithGameplaySystems();

        [TearDown]
        public void TearDown() => _board.Dispose();

        private void SolveAndSettle()
        {
            _board.Swipe(SolvingTile, -1, 0);
            _board.Time.RealtimeSinceStartup = 83f;
            _board.Systems.Tick(4);
        }

        [Test]
        public void Movement_KeepsInputLockedAcrossFrames()
        {
            _board.Swipe(SolvingTile, -1, 0);

            Assert.That(_board.Tiles[SolvingTile].Has<MoveComponent>(), Is.True);
            Assert.That(_board.Session.IsCompleted, Is.False, "the last move is still animating");

            _board.Swipe(SolvingTile, -1, 0);
            Assert.That(_board.Tiles[SolvingTile].Get<TileComponent>().Cell, Is.EqualTo(SolvingTile), "a second swipe during movement is ignored");
        }

        [Test]
        public void Win_StartsAfterTheFinalMovement_AndRecordsMovesAndTime()
        {
            SolveAndSettle();

            Assert.That(_board.Tiles[SolvingTile].Has<MoveComponent>(), Is.False);
            Assert.That(_board.Session.IsCompleted, Is.True);
            Assert.That(_board.Session.LastGameResult.TurnCount, Is.EqualTo(1));
            Assert.That(_board.Session.LastGameResult.GameTime, Is.EqualTo(TimeSpan.FromSeconds(83)));
        }

        [Test]
        public void SolvedBoard_RejectsMoves_AndStaysVisibleDuringTheResultDelay()
        {
            SolveAndSettle();
            _board.Swipe(SolvingTile, -1, 0);

            Assert.That(_board.Tiles[SolvingTile].Get<TileComponent>().Cell, Is.EqualTo(SolvingTile));
            Assert.That(_board.World.Count<GameEndEvent>(), Is.Zero, "no early cleanup");
        }

        [Test]
        public void ResultDelay_EmitsResultAndCleanupOnce()
        {
            SolveAndSettle();
            _board.Systems.Tick(30);

            Assert.That(_board.World.Count<GameEndEvent>(), Is.EqualTo(1));
            Assert.That(_board.World.Count<ChangeStateEvent>(), Is.EqualTo(1));
        }

        [Test]
        public void NextRun_ResetsCompletion()
        {
            SolveAndSettle();
            _board.State.Get<GameStateComponent>().CurrentState = GameStateType.MainMenu;
            _board.Systems.Run();
            _board.Session.EndRun();
            _board.Session.BeginRun();

            Assert.That(_board.Session.IsCompleted, Is.False);
            Assert.That(_board.Session.IsRunning, Is.True);
        }

        [Test]
        public void ManualExit_WinsOverTheResultDelay()
        {
            SolveAndSettle();
            _board.Systems.Tick(10); // Well inside the two-second result delay.
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Tick(5);

            Assert.That(_board.World.Count<ChangeStateEvent>(), Is.Zero, "no stale Finished request after exit");
        }
    }
}
