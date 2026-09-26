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

        private void Build(int columns, int rows, bool withCleanup)
        {
            _board = new BoardFixture(columns, rows);
            _board.Systems = new EcsSystems(_board.World).Add(new BoardSetupSystem());
            if (withCleanup)
                _board.Systems.Add(new BoardDestroySystem()).OneFrame<GameEndEvent>();
            _board.Systems.Inject(_board.Session).Inject(_board.Time);
            _board.Systems.Init();
            _boards = (EcsFilter<BoardComponent, BoardHistoryComponent>)_board.World.GetFilter(typeof(EcsFilter<BoardComponent, BoardHistoryComponent>));
        }

        private void RequestRun() => _board.World.NewEntity().Get<StartRunRequest>();

        [Test]
        public void Setup_WaitsForARequest_AndConsumesIt()
        {
            Build(3, 3, false);
            _board.Systems.Tick(2);
            Assert.That(_boards.GetEntitiesCount(), Is.Zero, "no request, no board");

            RequestRun();
            RequestRun();
            _board.Systems.Tick(2);
            Assert.That(_boards.GetEntitiesCount(), Is.EqualTo(1), "a duplicate request does not make a second board");
            Assert.That(_board.World.Count<StartRunRequest>(), Is.Zero);
        }

        [Test]
        public void Setup_CreatesOneReproducibleBoard()
        {
            Build(4, 3, false);
            RequestRun();
            _board.Systems.Tick(2);

            Assert.That(_boards.GetEntitiesCount(), Is.EqualTo(1));
            var layout = _boards.Get1(0).State;
            var expected = SeededShuffle.Create(4, 3, _boards.Get2(0).Seed);
            Assert.That((layout.Columns, layout.Rows), Is.EqualTo((4, 3)));
            Assert.That(layout.IsSolved, Is.False);
            Assert.That(Enumerable.Range(0, 12).All(cell => layout[cell] == expected[cell]), Is.True, "the seed recreates the layout");
        }

        [Test]
        public void Cleanup_RemovesBoardHistoryAndSelectionWithoutAView()
        {
            Build(3, 3, true);
            RequestRun();
            _board.Systems.Run();
            _boards.GetEntity(0).Get<TileSelectionComponent>();
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Run();

            Assert.That(_boards.GetEntitiesCount(), Is.Zero);
            Assert.That(_board.World.Count<TileSelectionComponent>(), Is.Zero);
        }

        [Test]
        public void Exit_DropsARequestThatWasNotPlayedYet()
        {
            Build(3, 3, true);
            RequestRun();
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Tick(2);

            Assert.That(_board.World.Count<StartRunRequest>(), Is.Zero);
            Assert.That(_boards.GetEntitiesCount(), Is.Zero);
        }

        [Test]
        public void EndedRun_CannotRecreateABoard_UntilTheNextRun()
        {
            Build(3, 3, true);
            RequestRun();
            _board.Systems.Run();
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Tick(2);
            Assert.That(_boards.GetEntitiesCount(), Is.Zero, "the Playing state may still be waiting for navigation");

            RequestRun();
            _board.Systems.Run();
            Assert.That(_boards.GetEntitiesCount(), Is.EqualTo(1));
            Assert.That(_boards.Get2(0).Moves, Is.Empty);
            Assert.That(_boards.GetEntity(0).Has<TileSelectionComponent>(), Is.False);
        }
    }

    public sealed class WinFlowTests
    {
        private const int SolvingTile = 7; // Sits in cell 8; one swap left from solved.
        private BoardFixture _board;

        [SetUp]
        public void SetUp() => _board = new BoardFixture()
            .WithBoard(new BoardState(3, 3, new[] { 0, 1, 2, 3, 4, 5, 6, 8, 7 }))
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
            Assert.That(_board.Board.Has<BoardSolvedComponent>(), Is.False, "the last move is still animating");

            _board.Swipe(SolvingTile, -1, 0);
            Assert.That(_board.Tiles[SolvingTile].Get<TileComponent>().Cell, Is.EqualTo(SolvingTile), "a second swipe during movement is ignored");
        }

        [Test]
        public void Win_StartsAfterTheFinalMovement_AndRecordsMovesAndTime()
        {
            SolveAndSettle();

            Assert.That(_board.Tiles[SolvingTile].Has<MoveComponent>(), Is.False);
            Assert.That(_board.Board.Has<BoardSolvedComponent>(), Is.True);
            Assert.That(_board.Session.LastGameResult.TurnCount, Is.EqualTo(1));
            Assert.That(_board.Session.LastGameResult.GameTime, Is.EqualTo(TimeSpan.FromSeconds(83)));
        }

        [Test]
        public void Win_AnnouncesTheSolvedBoardOnce()
        {
            SolveAndSettle();
            _board.Systems.Tick(30); // The fixture keeps events, so this counts every send.

            Assert.That(_board.World.Count<BoardSolvedEvent>(), Is.EqualTo(1));
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
        public void NextRun_StartsWithAFreshResult()
        {
            SolveAndSettle();
            _board.Session.BeginRun();

            Assert.That(_board.Session.LastGameResult.TurnCount, Is.Zero);
            Assert.That(_board.Session.LastGameResult.GameTime, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void Win_RecordsThePuzzleCompleted_AndSaves_BeforeTheResultScreen()
        {
            SolveAndSettle();

            Assert.That(_board.Progress.IsCompleted(_board.Puzzle), Is.True);
            Assert.That(_board.World.Count<SaveDataEvent>(), Is.Not.Zero);
            Assert.That(_board.World.Count<ChangeStateEvent>(), Is.Zero, "the result screen is still two seconds away");
        }

        [Test]
        public void ExitDuringTheResultDelay_KeepsThePuzzleCompleted()
        {
            SolveAndSettle();
            _board.World.NewEntity().Get<GameEndEvent>(); // the header's Back, inside the pause
            _board.Systems.Tick(30);

            Assert.That(_board.World.Count<ChangeStateEvent>(), Is.Zero, "no result screen after the exit");
            Assert.That(_board.Progress.IsCompleted(_board.Puzzle), Is.True);
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
