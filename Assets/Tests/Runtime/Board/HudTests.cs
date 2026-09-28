using Fives.Components;
using Fives.Domain;
using Fives.Models;
using Fives.Systems;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Board
{
    /// <summary>Счётчик ходов и кнопки Undo и Hint на экране партии.</summary>
    public sealed class HudTests
    {
        private BoardFixture _board;
        private FakeGamePlayView _view;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardFixture().WithSeededBoard(42);
            _board.Session.SetSelectedImage(new PuzzleData { Id = "dogs.corgi" }, null);
            var presenter = _board.Game.GamePlay();
            _view = new FakeGamePlayView();
            presenter.Initialize(_view);
            _board.Systems = new EcsSystems(_board.World).Add(new BoardHudSystem(presenter)).Inject(_board.Session).Inject(_board.Game.Balance);
            _board.Systems.Init();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        /// <summary>
        /// 1000 кадров без изменений:
        /// экран обновился один раз, а кадр без изменений не выделяет память.
        /// </summary>
        [Test]
        public void IdleFrames_UpdateTheViewOnce_AndDoNotAllocate()
        {
            _board.Systems.Tick(1000);

            Assert.That(Allocations.During(_board.Systems.Run), Is.Zero);
            Assert.That(_view.Updates, Is.EqualTo(1));
            Assert.That(_view.Moves, Is.EqualTo("game.moves:0"));
        }

        /// <summary>Новый ход обновляет счётчик ходов и включает кнопки Undo и Hint.</summary>
        [Test]
        public void HistoryChange_UpdatesTheView()
        {
            _board.Systems.Run();
            _board.Moves.Add(new Swap(0, 1));
            _board.Systems.Run();

            Assert.That(_view.Updates, Is.EqualTo(2));
            Assert.That(_view.Moves, Is.EqualTo("game.moves:1"));
            Assert.That(_view.CanUndo, Is.True);
            Assert.That(_view.CanHint, Is.True);
        }

        /// <summary>На кнопке подсказки видна цена; пока подсказка горит, кнопка выключена.</summary>
        [Test]
        public void ActiveHint_DisablesTheHintButton_AndShowsThePrice()
        {
            _board.Systems.Run();
            Assert.That(_view.HintPrice, Is.EqualTo(TestConfig.HintPrice.ToString()));

            _board.Board.Replace(new BoardHintComponent { TileId = 0 });
            _board.Systems.Run();

            Assert.That(_view.CanHint, Is.False);
            Assert.That(_view.Updates, Is.EqualTo(2));
        }
    }
}
