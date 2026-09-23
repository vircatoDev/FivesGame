using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class GameResultPresenter : Presenter<GameResultView>
    {
        private readonly GameSession _gameSession;
        private readonly StarService _starService;
        private readonly PlayerProgressService _playerProgressService;
        private readonly EcsWorld _world;

        public GameResultPresenter(
            GameSession gameSession,
            StarService starService,
            PlayerProgressService playerProgressService,
            EcsWorld world)
        {
            _gameSession = gameSession;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _world = world;
        }

        public override void OnActivateView()
        {
            PlayOpenPopUpAudioEffects();
            UpdateProgress();
            SavePlayerProgress();
            View.UpdateViewContent(_gameSession.LastGameResult, _gameSession.SelectedTheme, _gameSession.SelectedPuzzle);
            View.PlayShowAnimation();
        }

        public void GetReward(bool doubleReward)
        {
            if (!_gameSession.TryClaimReward(doubleReward, out var rewardAmount))
            {
                return;
            }

            GiveReward(rewardAmount);
            SaveReward();
            BackToMainMenu();
        }

        private void GiveReward(int rewardAmount)
        {
            _starService.Add(rewardAmount);
            _world.Send(CurrencyChangedEvent.Changed(Currency.Stars, _starService.GetBalance(), rewardAmount));
        }

  
        private void UpdateProgress() => _playerProgressService.MarkPuzzleCompleted(_gameSession.SelectedPuzzle.Name);
        private void SavePlayerProgress() => _world.Send(new SaveDataEvent { StorableObject = _playerProgressService });
        private void SaveReward() => _world.Send(new SaveDataEvent { StorableObject = _starService });
        private void PlayOpenPopUpAudioEffects() => _world.PlaySound(AudioKeyCollection.OpenPopUp);
        private void BackToMainMenu() => _world.ChangeState(GameStateType.MainMenu);
    }
}
