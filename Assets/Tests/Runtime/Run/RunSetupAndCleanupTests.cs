using System.Linq;
using Fives.Components;
using Fives.Domain;
using Fives.Systems;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Run
{
    /// <summary>Поле партии появляется один раз на запрос старта и убирается вместе с партией.</summary>
    public sealed class RunSetupAndCleanupTests
    {
        private BoardFixture _board;
        private EcsFilter<BoardComponent, BoardHistoryComponent> _boards;

        [TearDown]
        public void TearDown() => _board.Dispose();

        private void Build(bool withCleanup)
        {
            _board = new BoardFixture();
            _board.Systems = new EcsSystems(_board.World).Add(new BoardSetupSystem());
            if (withCleanup)
                _board.Systems.Add(new BoardDestroySystem()).OneFrame<GameEndEvent>();
            _board.Systems.Inject(_board.Session).Inject(_board.Time);
            _board.Systems.Init();
            _boards = (EcsFilter<BoardComponent, BoardHistoryComponent>)_board.World.GetFilter(typeof(EcsFilter<BoardComponent, BoardHistoryComponent>));
        }

        private void RequestRun() => _board.World.NewEntity().Get<StartRunRequest>();

        /// <summary>
        /// Без запроса старта поле не создаётся;
        /// два запроса подряд дают одно поле, запрос после этого убирается.
        /// </summary>
        [Test]
        public void Setup_WaitsForARequest_AndConsumesIt()
        {
            Build(withCleanup: false);
            _board.Systems.Tick(2);
            Assert.That(_boards.GetEntitiesCount(), Is.Zero, "no request, no board");

            RequestRun();
            RequestRun();
            _board.Systems.Tick(2);
            Assert.That(_boards.GetEntitiesCount(), Is.EqualTo(1), "a duplicate request does not make a second board");
            Assert.That(_board.World.Count<StartRunRequest>(), Is.Zero);
        }

        /// <summary>Создаётся одно перемешанное поле 4×3, и по его seed раскладку можно воспроизвести.</summary>
        [Test]
        public void Setup_CreatesOneReproducibleBoard()
        {
            Build(withCleanup: false);
            RequestRun();
            _board.Systems.Tick(2);

            Assert.That(_boards.GetEntitiesCount(), Is.EqualTo(1));
            var layout = _boards.Get1(0).State;
            var expected = SeededShuffle.Create(4, 3, _boards.Get2(0).Seed);
            Assert.That((layout.Columns, layout.Rows), Is.EqualTo((4, 3)));
            Assert.That(layout.IsSolved, Is.False);
            Assert.That(Enumerable.Range(0, 12).All(cell => layout[cell] == expected[cell]), Is.True, "the seed recreates the layout");
        }

        /// <summary>
        /// Выход из партии убирает поле, историю ходов и выбор плитки, даже если плиток на экране нет.
        /// </summary>
        [Test]
        public void Cleanup_RemovesBoardHistoryAndSelectionWithoutAView()
        {
            Build(withCleanup: true);
            RequestRun();
            _board.Systems.Run();
            _boards.GetEntity(0).Get<TileSelectionComponent>();
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Run();

            Assert.That(_boards.GetEntitiesCount(), Is.Zero);
            Assert.That(_board.World.Count<TileSelectionComponent>(), Is.Zero);
        }

        /// <summary>Выход до того, как поле создано, отбрасывает запрос старта: поле не появится.</summary>
        [Test]
        public void Exit_DropsARequestThatWasNotPlayedYet()
        {
            Build(withCleanup: true);
            RequestRun();
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Systems.Tick(2);

            Assert.That(_board.World.Count<StartRunRequest>(), Is.Zero);
            Assert.That(_boards.GetEntitiesCount(), Is.Zero);
        }

        /// <summary>
        /// После выхода старое поле не возвращается;
        /// следующий старт даёт новое поле с пустой историей.
        /// </summary>
        [Test]
        public void EndedRun_CannotRecreateABoard_UntilTheNextRun()
        {
            Build(withCleanup: true);
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
}
