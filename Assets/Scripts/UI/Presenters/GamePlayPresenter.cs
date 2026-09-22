using Leopotam.Ecs;
using Scripts.Commands;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class GamePlayPresenter : BasePresenter
    {
        private readonly GameSession _gameSession;
        private readonly ECSCommandService _ecsCommandService;
        private readonly EcsWorld _world;
        private readonly EcsFilter<BoardComponent, BoardHistoryComponent> _boards;
        private readonly EcsFilter<BoardReplayComponent> _replays;
        private readonly EcsFilter<TileComponent, MoveComponent> _moves;
        private GamePlayView _view;
        private (int Seed, int Moves, bool Replaying, int Position)? _statusKey;
        private string _status = "";

        public GamePlayPresenter(GameSession gameSession, ECSCommandService ecsCommandService, EcsWorld world)
        {
            _world = world;
            _boards = (EcsFilter<BoardComponent, BoardHistoryComponent>)world.GetFilter(typeof(EcsFilter<BoardComponent, BoardHistoryComponent>));
            _replays = (EcsFilter<BoardReplayComponent>)world.GetFilter(typeof(EcsFilter<BoardReplayComponent>));
            _moves = (EcsFilter<TileComponent, MoveComponent>)world.GetFilter(typeof(EcsFilter<TileComponent, MoveComponent>));
            _gameSession = gameSession;
            _ecsCommandService = ecsCommandService;
        }

        public override void Initialize(BaseView initData)
        {
            _view = initData as GamePlayView;
            OnActivateView();
        }

        public override void OnActivateView()
        {
            _view.UpdateViewContent(_gameSession.SelectedPuzzle);
            _view.PlayShowAnimation();

            _ecsCommandService.CreateCommand<UpdateHeaderBtnLogicCommand>(BackToMainMenu, HeaderBtnType.Back).Execute();
        }

        public void RequestControl(BoardControl control) =>
            _world.NewEntity().Replace(new BoardControlEvent { Control = control });

        public void RefreshControls()
        {
            if (_boards.GetEntitiesCount() != 1)
            {
                _statusKey = null;
                _view.UpdateControls("", false, false, false);
                return;
            }

            var history = _boards.Get2(0);
            var replaying = _replays.GetEntitiesCount() > 0;
            var ready = _gameSession.IsRunning && !_gameSession.IsCompleted;
            var canEdit = ready && !replaying && _moves.GetEntitiesCount() == 0 && history.Moves.Count > 0;
            var position = replaying ? _replays.Get1(0).Position : 0;
            var key = (history.Seed, history.Moves.Count, replaying, position);
            if (_statusKey != key)
            {
                _statusKey = key;
                _status = replaying
                    ? $"Повтор: {position}/{history.Moves.Count}"
                    : $"Ходов: {history.Moves.Count}  ·  Seed: {history.Seed}";
            }
            _view.UpdateControls(_status, canEdit, canEdit || (ready && replaying), replaying);
        }

        private void BackToMainMenu()
        {
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();
            _ecsCommandService.CreateSimpleEventCommand<GameEndEvent>().Execute();
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.MainMenu).Execute();
        }
    }
}
