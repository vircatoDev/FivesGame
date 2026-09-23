using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class GamePlayPresenter : Presenter<GamePlayView>
    {
        private readonly GameSession _gameSession;
        private readonly EcsWorld _world;

        public GamePlayPresenter(GameSession gameSession, EcsWorld world)
        {
            _world = world;
            _gameSession = gameSession;
        }

        public override void OnActivateView()
        {
            View.UpdateViewContent(_gameSession.SelectedPuzzle);
            View.PlayShowAnimation().Forget();

            _world.Send(new UpdateControlPanelBtnLogicEvent { CommonBtnCallback = BackToMainMenu, BtnType = HeaderBtnType.Back });
        }

        public void RequestControl(BoardControl control) =>
            _world.Send(new BoardControlEvent { Control = control });

        public void ShowHud(in BoardHud hud)
        {
            var status = hud.Replaying
                ? $"Повтор: {hud.ReplayPosition}/{hud.Moves}"
                : $"Ходов: {hud.Moves}  ·  Seed: {hud.Seed}";
            View.UpdateControls(status, hud.CanUndo, hud.CanReplay, hud.Replaying);
        }

        private void BackToMainMenu()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            _world.Send<GameEndEvent>();
            _world.ChangeState(GameStateType.MainMenu);
        }
    }
}
