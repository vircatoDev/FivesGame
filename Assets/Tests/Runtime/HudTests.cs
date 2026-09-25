using Fives.Domain;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
using Scripts.Models;
using Scripts.Systems;
using Scripts.UI.Presenters;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is; // Extends NUnit's Is with AllocatingGCMemory.

namespace Fives.Runtime.Tests
{
    public sealed class HudTests
    {
        private BoardFixture _board;
        private FakeGamePlayView _view;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardFixture().WithSeededBoard(42);
            _board.Session.SetSelectedImage(new PuzzleData { Id = "dogs.corgi" });
            var presenter = new GamePlayPresenter(_board.Session, new FakeHeaderPanelView(), _board.World, new FakeTexts());
            _view = new FakeGamePlayView();
            presenter.Initialize(_view);
            _board.Systems = new EcsSystems(_board.World).Add(new BoardHudSystem(presenter)).Inject(_board.Session).Inject(_board.Objects.Config());
            _board.Systems.Init();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        [Test]
        public void IdleFrames_UpdateTheViewOnce_AndDoNotAllocate()
        {
            _board.Systems.Tick(1000);

            Assert.That(() => _board.Systems.Run(), Is.Not.AllocatingGCMemory());
            Assert.That(_view.Updates, Is.EqualTo(1));
            Assert.That(_view.Moves, Is.EqualTo("game.moves:0"));
        }

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

        [Test]
        public void MovingTiles_DoNotToggleTheButtons()
        {
            _board.Moves.Add(new Swap(0, 1));
            _board.Systems.Run();
            var updates = _view.Updates;

            _board.Tiles[0].Get<MoveComponent>();
            _board.Systems.Tick(3);

            Assert.That(_view.Updates, Is.EqualTo(updates), "no refresh while a tile animates");
            Assert.That(_view.CanUndo && _view.CanHint, Is.True);
        }

        [Test]
        public void ActiveHint_DisablesTheHintButton_AndShowsThePrice()
        {
            _board.Systems.Run();
            Assert.That(_view.HintPrice, Is.EqualTo("5"));

            _board.Board.Replace(new BoardHintComponent { TileId = 0 });
            _board.Systems.Run();

            Assert.That(_view.CanHint, Is.False);
            Assert.That(_view.Updates, Is.EqualTo(2));
        }
    }
}
