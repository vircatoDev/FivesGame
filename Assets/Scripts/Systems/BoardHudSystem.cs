using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Systems
{
    /// <summary>
    /// Pushes the move counter and Undo/Hint availability to the gameplay HUD when they change. Tile animations do not
    /// count: input systems ignore presses while tiles move, so the buttons do not flicker on every move.
    /// </summary>
    public class BoardHudSystem : IEcsRunSystem
    {
        private readonly GameBalance _balance = null;
        private readonly EcsFilter<BoardHistoryComponent> _boards = null;
        private readonly IBoardHud _hud;
        private BoardHud _shown;

        public BoardHudSystem(IBoardHud hud)
        {
            _hud = hud;
        }

        public void Run()
        {
            if (_boards.GetEntitiesCount() != 1)
                return;

            ref var history = ref _boards.Get1(0);
            var board = _boards.GetEntity(0);
            var ready = !board.Has<BoardSolvedComponent>();
            var hud = new BoardHud(history.Seed, history.Moves.Count, ready && history.Moves.Count > 0,
                ready && !board.Has<BoardHintComponent>(), _balance.HintPrice);

            if (hud.Equals(_shown))
                return;

            _shown = hud;
            _hud.Show(hud);
        }
    }
}
