using Scripts.Commands;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class GamePlayPresenter : BasePresenter
    {
        private readonly GameSession _gameSession;
        private readonly ECSCommandService _ecsCommandService;
        private GamePlayView _view;

        public GamePlayPresenter(GameSession gameSession, ECSCommandService ecsCommandService)
        {
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

        private void BackToMainMenu()
        {
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();
            _ecsCommandService.CreateCommand<EndGameCommand>(0).Execute();
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.MainMenu).Execute();
        }
    }
}