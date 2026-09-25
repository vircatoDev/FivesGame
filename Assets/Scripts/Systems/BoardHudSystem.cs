using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.UI.Presenters;

namespace Scripts.Systems
{
    /// <summary>Pushes the move counter and Undo/Hint availability to the gameplay HUD when they change.</summary>
    public class BoardHudSystem : IEcsRunSystem
    {
        private readonly GameSession _session = null;
        private readonly GlobalConfig _config = null;
        private readonly EcsFilter<BoardHistoryComponent> _boards = null;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves = null;
        private readonly GamePlayPresenter _presenter;
        private BoardHud _shown;

        public BoardHudSystem(GamePlayPresenter presenter)
        {
            _presenter = presenter;
        }

        public void Run()
        {
            if (_boards.GetEntitiesCount() != 1)
                return;

            ref var history = ref _boards.Get1(0);
            var ready = _session.IsRunning && !_session.IsCompleted && _moves.GetEntitiesCount() == 0;
            var hud = new BoardHud(history.Seed, history.Moves.Count, ready && history.Moves.Count > 0,
                ready && !_boards.GetEntity(0).Has<BoardHintComponent>(), _config.HintPrice);

            if (hud.Equals(_shown))
                return;

            _shown = hud;
            _presenter.ShowHud(hud);
        }
    }
}
