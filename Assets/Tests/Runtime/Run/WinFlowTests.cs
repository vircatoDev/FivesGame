using System;
using Fives.Components;
using Fives.Domain;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Run
{
    /// <summary>От последнего хода до окна результата: победа, её запись и пауза в две секунды.</summary>
    public sealed class WinFlowTests
    {
        private const int SolvingTile = 10; // стоит в клетке 11, до сборки остался один обмен
        private BoardFixture _board;

        [SetUp]
        public void SetUp() => _board = new BoardFixture()
            .WithBoard(new BoardState(4, 3, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 10 }))
            .WithGameplaySystems();

        [TearDown]
        public void TearDown() => _board.Dispose();

        private void SolveAndSettle()
        {
            _board.Swipe(SolvingTile, -1, 0);
            _board.Time.RealtimeSinceStartup = 83f;
            _board.Systems.Tick(4);
        }

        /// <summary>Пока последняя плитка едет, победа ещё не засчитана, а новый свайп игнорируется.</summary>
        [Test]
        public void Movement_KeepsInputLockedAcrossFrames()
        {
            _board.Swipe(SolvingTile, -1, 0);

            Assert.That(_board.Tiles[SolvingTile].Has<MoveComponent>(), Is.True);
            Assert.That(_board.Board.Has<BoardSolvedComponent>(), Is.False, "the last move is still animating");

            _board.Swipe(SolvingTile, -1, 0);
            Assert.That(_board.Tiles[SolvingTile].Get<TileComponent>().Cell, Is.EqualTo(SolvingTile), "a second swipe during movement is ignored");
        }

        /// <summary>
        /// Победа засчитывается, когда последняя плитка доехала;
        /// в результат пишутся 1 ход и 83 секунды.
        /// </summary>
        [Test]
        public void Win_StartsAfterTheFinalMovement_AndRecordsMovesAndTime()
        {
            SolveAndSettle();

            Assert.That(_board.Tiles[SolvingTile].Has<MoveComponent>(), Is.False);
            Assert.That(_board.Board.Has<BoardSolvedComponent>(), Is.True);
            Assert.That(_board.Session.LastGameResult.TurnCount, Is.EqualTo(1));
            Assert.That(_board.Session.LastGameResult.GameTime, Is.EqualTo(TimeSpan.FromSeconds(83)));
        }

        /// <summary>О собранном поле сообщается один раз, сколько бы кадров ни прошло.</summary>
        [Test]
        public void Win_AnnouncesTheSolvedBoardOnce()
        {
            SolveAndSettle();
            _board.Systems.Tick(30); // фикстура не удаляет события, поэтому считаются все отправки

            Assert.That(_board.World.Count<BoardSolvedEvent>(), Is.EqualTo(1));
        }

        /// <summary>Пазл отмечается собранным и сохраняется сразу, ещё до окна результата.</summary>
        [Test]
        public void Win_RecordsThePuzzleCompleted_AndSaves_BeforeTheResultScreen()
        {
            SolveAndSettle();

            Assert.That(_board.Progress.IsCompleted(_board.Puzzle), Is.True);
            Assert.That(_board.World.Count<SaveDataEvent>(), Is.Not.Zero);
            Assert.That(_board.World.Count<ChangeStateEvent>(), Is.Zero, "the result screen is still two seconds away");
        }

        /// <summary>
        /// На собранном поле ходы не принимаются, а само поле остаётся на экране до окна результата.
        /// </summary>
        [Test]
        public void SolvedBoard_RejectsMoves_AndStaysVisibleDuringTheResultDelay()
        {
            SolveAndSettle();
            _board.Swipe(SolvingTile, -1, 0);

            Assert.That(_board.Tiles[SolvingTile].Get<TileComponent>().Cell, Is.EqualTo(SolvingTile));
            Assert.That(_board.World.Count<GameEndEvent>(), Is.Zero, "no early cleanup");
        }

        /// <summary>Через две секунды партия заканчивается и открывается окно результата, один раз.</summary>
        [Test]
        public void ResultDelay_EmitsResultAndCleanupOnce()
        {
            SolveAndSettle();
            _board.Systems.Tick(30);

            Assert.That(_board.World.Count<GameEndEvent>(), Is.EqualTo(1));
            Assert.That(_board.World.Count<ChangeStateEvent>(), Is.EqualTo(1));
        }

        /// <summary>Выход в паузе после победы: окно результата не открывается, но пазл остаётся собранным.</summary>
        [Test]
        public void ExitDuringTheResultDelay_OpensNoResult_AndKeepsThePuzzleCompleted()
        {
            SolveAndSettle();
            _board.Systems.Tick(10); // внутри двухсекундной паузы
            _board.World.NewEntity().Get<GameEndEvent>(); // Back в шапке
            _board.Systems.Tick(30);

            Assert.That(_board.World.Count<ChangeStateEvent>(), Is.Zero, "no stale Finished request after the exit");
            Assert.That(_board.Progress.IsCompleted(_board.Puzzle), Is.True);
        }

        /// <summary>Следующая партия начинается с чистого результата: 0 ходов и 0 секунд.</summary>
        [Test]
        public void NextRun_StartsWithAFreshResult()
        {
            SolveAndSettle();
            _board.Session.BeginRun();

            Assert.That(_board.Session.LastGameResult.TurnCount, Is.Zero);
            Assert.That(_board.Session.LastGameResult.GameTime, Is.EqualTo(TimeSpan.Zero));
        }
    }
}
