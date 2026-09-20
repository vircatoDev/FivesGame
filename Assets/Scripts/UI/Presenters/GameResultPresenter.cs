using Scripts.Commands;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.UI.Presenters
{
    public class GameResultPresenter : BasePresenter
    {
        private readonly GameSession _gameSession;
        private readonly StarService _starService;
        private readonly PlayerProgressService _playerProgressService;
        private readonly ECSCommandService _ecsCommandService;
        private GameResultView _view;

        public GameResultPresenter(
            GameSession gameSession,
            StarService starService,
            PlayerProgressService playerProgressService,
            ECSCommandService ecsCommandService)
        {
            _gameSession = gameSession;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _ecsCommandService = ecsCommandService;
        }

        public override void Initialize(BaseView initData)
        {
            _view = initData as GameResultView;
            if (_view == null)
            {
                Debug.LogError("GameResultPresenter: wrong initData.");
                return;
            }

            OnActivateView();
        }

        public override void OnActivateView()
        {
            PlayOpenPopUpAudioEffects();
            UpdateProgress();
            SavePlayerProgress();
            _view.UpdateViewContent(_gameSession.LastGameResult, _gameSession.SelectedTheme, _gameSession.SelectedPuzzle);
            _view.PlayShowAnimation();
        }

        public void GetReward(bool doubleReward)
        {
            GiveReward(doubleReward);
            SaveReward();
            BackToMainMenu();
        }

        private void GiveReward(bool doubleReward)
        {
            int rewardAmount = doubleReward ? 20 : 10;
            _starService.Add(rewardAmount);
            _ecsCommandService.CreateCommand<UpdateStarBalanceCommand>(_starService,rewardAmount).Execute();
        }

  
        private void UpdateProgress() => _playerProgressService.MarkPuzzleCompleted(_gameSession.SelectedPuzzle.Name);
        private void SavePlayerProgress() => _ecsCommandService.CreateCommand<SaveDataCommand>(_playerProgressService).Execute();
        private void SaveReward() => _ecsCommandService.CreateCommand<SaveDataCommand>(_starService).Execute();
        private void PlayOpenPopUpAudioEffects() => _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.OpenPopUp,1f).Execute();
        private void BackToMainMenu() => _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.MainMenu).Execute();
    }
}